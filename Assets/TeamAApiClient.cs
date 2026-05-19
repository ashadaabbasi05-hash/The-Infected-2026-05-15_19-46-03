using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public sealed class TeamAApiClient : MonoBehaviour
{
    public static TeamAApiClient Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] string baseUrl = "http://localhost:8000";
    [SerializeField, Min(0.1f)] float requestTimeoutSeconds = 8f;
    [SerializeField] bool enableApiCalls = false;
    [SerializeField] bool debugLogs = true;
    [SerializeField] bool traceToAgentPanel = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (IsValidInstance(Instance))
            {
                if (debugLogs)
                {
                    Debug.LogWarning("[TEAM A API] Duplicate TeamAApiClient detected. Disabling this instance.", this);
                }

                enabled = false;
                return;
            }

            Instance = null;
        }

        Instance = this;
        baseUrl = (baseUrl ?? string.Empty).Trim().TrimEnd('/');

        if (debugLogs)
        {
            Debug.Log($"[TEAM A API] Mode: {(enableApiCalls ? "online" : "local fallback")}. Base URL: {baseUrl}", this);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public IEnumerator Health(Action<bool, HealthResponse> callback)
    {
        yield return SendGet("/health", callback);
    }

    public IEnumerator RegisterBot(RegisterBotRequest request, Action<bool, RegisterBotResponse> callback)
    {
        yield return SendPost("/v4/register-bot", request, callback);
    }

    public IEnumerator UnregisterBot(UnregisterBotRequest request, Action<bool, UnregisterBotResponse> callback)
    {
        yield return SendPost("/v4/unregister-bot", request, callback);
    }

    public IEnumerator DecideAction(DecideActionRequest request, Action<bool, DecideActionResponse> callback)
    {
        yield return SendPost("/v4/decide-action", request, callback);
    }

    public IEnumerator Respond(RespondRequest request, Action<bool, RespondResponse> callback)
    {
        yield return SendPost("/v4/respond", request, callback);
    }

    public IEnumerator Vote(VoteRequest request, Action<bool, VoteResponse> callback)
    {
        yield return SendPost("/v4/vote", request, callback);
    }

    string BuildUrl(string path)
    {
        string sanitizedPath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim();
        if (!sanitizedPath.StartsWith("/", StringComparison.Ordinal))
        {
            sanitizedPath = "/" + sanitizedPath;
        }

        return $"{baseUrl}{sanitizedPath}";
    }

    UnityWebRequest CreatePostRequest(string path, string json)
    {
        string payload = json ?? "{}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

        UnityWebRequest request = new UnityWebRequest(BuildUrl(path), UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = Mathf.CeilToInt(requestTimeoutSeconds)
        };

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        return request;
    }

    IEnumerator SendGet<T>(string path, Action<bool, T> callback) where T : class
    {
        if (!enableApiCalls)
        {
            LogDisabledFallback();
            callback?.Invoke(false, null);
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(BuildUrl(path));
        request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            LogFailure(path, request.error);
            TraceImportant($"{path} failed. Local fallback active.");
            callback?.Invoke(false, null);
            yield break;
        }

        if (!TryParseJson(GetResponseText(request), out T result))
        {
            LogFailure(path, "invalid json");
            callback?.Invoke(false, null);
            yield break;
        }

        LogSuccess(path);
        TraceImportant($"{path} success.");
        callback?.Invoke(true, result);
    }

    IEnumerator SendPost<TRequest, TResponse>(string path, TRequest requestData, Action<bool, TResponse> callback)
        where TResponse : class
    {
        if (!enableApiCalls)
        {
            LogDisabledFallback();
            callback?.Invoke(false, null);
            yield break;
        }

        string json = "{}";
        try
        {
            if (requestData != null)
            {
                json = JsonUtility.ToJson(requestData);
            }
        }
        catch (Exception ex)
        {
            LogFailure(path, $"request serialization failed: {ex.Message}");
            callback?.Invoke(false, null);
            yield break;
        }

        using UnityWebRequest webRequest = CreatePostRequest(path, json);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            LogFailure(path, webRequest.error);
            TraceImportant($"{path} failed. Local fallback active.");
            callback?.Invoke(false, null);
            yield break;
        }

        if (!TryParseJson(GetResponseText(webRequest), out TResponse responseData))
        {
            LogFailure(path, "invalid json");
            callback?.Invoke(false, null);
            yield break;
        }

        LogSuccess(path);
        callback?.Invoke(true, responseData);
    }

    bool TryParseJson<T>(string json, out T result) where T : class
    {
        result = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            result = JsonUtility.FromJson<T>(json);
            return result != null;
        }
        catch (Exception ex)
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[TEAM A API] JSON parse failed: {ex.Message}", this);
            }

            return false;
        }
    }

    static string GetResponseText(UnityWebRequest request)
    {
        return request != null && request.downloadHandler != null ? request.downloadHandler.text : null;
    }

    void LogSuccess(string path)
    {
        if (debugLogs)
        {
            Debug.Log($"[TEAM A API] {path} success", this);
        }
    }

    void LogFailure(string path, string error)
    {
        if (debugLogs)
        {
            Debug.LogWarning($"[TEAM A API] {path} failed: {error}", this);
        }
    }

    void LogDisabledFallback()
    {
        if (debugLogs)
        {
            Debug.Log("[TEAM A API] API calls disabled. Using local fallback.", this);
        }
    }

    void TraceImportant(string message)
    {
        if (!traceToAgentPanel || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        AgentTracePanel.Trace("API", message);
    }

    static bool IsValidInstance(TeamAApiClient instance)
    {
        try
        {
            return instance != null && instance.gameObject != null;
        }
        catch
        {
            return false;
        }
    }

    public static string LocalBehaviorForWave(int wave, bool isFinalChase)
    {
        if (isFinalChase)
        {
            return "final_hunt";
        }

        if (wave <= 1)
        {
            return "stealth_fake_task";
        }

        if (wave == 2)
        {
            return "stalk";
        }

        return "aggressive_chase";
    }

    public static string LocalRandomVoteTarget(string[] humanPlayers)
    {
        if (humanPlayers == null || humanPlayers.Length == 0)
        {
            return null;
        }

        int randomHumanIndex = UnityEngine.Random.Range(0, humanPlayers.Length);
        return humanPlayers[randomHumanIndex];
    }

    public static RespondResponse LocalSilentRespond(string botId)
    {
        return new RespondResponse
        {
            botId = botId,
            respond = false,
            messages = Array.Empty<string>(),
            typingDelaySeconds = 0f,
            secondMessageDelaySeconds = 0f,
            trace = "Local fallback: bot stayed silent."
        };
    }
}
