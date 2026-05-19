using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TeamAApiDebugTester : MonoBehaviour
{
    [SerializeField] TeamAApiClient apiClient;
    [SerializeField] bool enableHotkeys = true;

    void Update()
    {
        if (!enableHotkeys)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            StartCoroutine(RunHealthTest());
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            StartCoroutine(RunRegisterTest());
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            StartCoroutine(RunRespondTest());
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            StartCoroutine(RunVoteTest());
        }
    }

    IEnumerator RunHealthTest()
    {
        TeamAApiClient client = ResolveClient();
        if (client == null)
        {
            yield break;
        }

        bool completed = false;
        bool success = false;

        yield return client.Health((ok, response) =>
        {
            completed = true;
            success = ok && response != null;
        });

        if (!completed)
        {
            yield break;
        }

        if (success)
        {
            AgentTracePanel.Trace("API", "Backend health OK.");
        }
        else
        {
            AgentTracePanel.Trace("API", "Backend unavailable. Local fallback active.");
        }
    }

    IEnumerator RunRegisterTest()
    {
        TeamAApiClient client = ResolveClient();
        if (client == null)
        {
            yield break;
        }

        RegisterBotRequest request = new RegisterBotRequest
        {
            matchId = "debug-match",
            botId = "bot-01",
            botName = "Debug Bot",
            wave = 1,
            cycle = 1,
            phase = "safe",
            alivePlayers = new[] { "HumanA", "HumanB", "Debug Bot" },
            humanPlayers = new[] { "HumanA", "HumanB" },
            infectedPlayers = new[] { "Debug Bot" },
            taskProgress = 25
        };

        yield return client.RegisterBot(request, (ok, response) =>
        {
            string detail = ok && response != null
                ? $"Register ok: {response.behaviorMode}"
                : "Register fallback used.";
            AgentTracePanel.Trace("API", detail);
        });
    }

    IEnumerator RunRespondTest()
    {
        TeamAApiClient client = ResolveClient();
        if (client == null)
        {
            yield break;
        }

        RespondRequest request = new RespondRequest
        {
            matchId = "debug-match",
            phase = "discussion",
            wave = 2,
            cycle = 1,
            botId = "bot-01",
            botName = "Debug Bot",
            personality = "calm",
            message = "Where is everyone?",
            latestMessage = new ChatMessageDto
            {
                sender = "human",
                senderName = "HumanA",
                text = "I saw someone near MedBay."
            },
            recentChat = new[]
            {
                new ChatMessageDto { sender = "human", senderName = "HumanA", text = "I saw someone near MedBay." }
            },
            alivePlayers = new[] { "HumanA", "HumanB", "Debug Bot" },
            humanPlayers = new[] { "HumanA", "HumanB" },
            infectedPlayers = new[] { "Debug Bot" }
        };

        yield return client.Respond(request, (ok, response) =>
        {
            string detail = ok && response != null
                ? "Respond endpoint returned data."
                : "Respond fallback used.";
            AgentTracePanel.Trace("API", detail);
        });
    }

    IEnumerator RunVoteTest()
    {
        TeamAApiClient client = ResolveClient();
        if (client == null)
        {
            yield break;
        }

        VoteRequest request = new VoteRequest
        {
            matchId = "debug-match",
            phase = "vote",
            wave = 2,
            cycle = 1,
            botId = "bot-01",
            botName = "Debug Bot",
            alivePlayers = new[] { "HumanA", "HumanB", "Debug Bot" },
            humanPlayers = new[] { "HumanA", "HumanB" },
            infectedPlayers = new[] { "Debug Bot" },
            recentChat = new[]
            {
                new ChatMessageDto { sender = "human", senderName = "HumanB", text = "Vote HumanA." }
            }
        };

        yield return client.Vote(request, (ok, response) =>
        {
            string detail = ok && response != null
                ? $"Vote target: {response.voteTarget}"
                : "Vote fallback used.";
            AgentTracePanel.Trace("API", detail);
        });
    }

    TeamAApiClient ResolveClient()
    {
        if (apiClient == null)
        {
            apiClient = TeamAApiClient.Instance;
        }

        if (apiClient == null)
        {
            Debug.LogWarning("[TEAM A API] TeamAApiDebugTester could not find TeamAApiClient.", this);
        }

        return apiClient;
    }
}
