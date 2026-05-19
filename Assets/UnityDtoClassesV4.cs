using System;

[Serializable]
public class HealthResponse
{
    public string status;
    public string service;
    public string contractVersion;
    public string aiMode;
    public string llmProvider;
    public bool firebaseConfigured;
}

[Serializable]
public class RegisterBotRequest
{
    public string matchId;
    public string botId;
    public string botName;
    public int wave;
    public int cycle;
    public string phase;
    public string[] alivePlayers;
    public string[] humanPlayers;
    public string[] infectedPlayers;
    public int taskProgress;
}

[Serializable]
public class RegisterBotResponse
{
    public bool ok;
    public string botId;
    public string personality;
    public string behaviorMode;
    public string trace;
}

[Serializable]
public class UnregisterBotRequest
{
    public string matchId;
    public string botId;
    public string reason;
}

[Serializable]
public class UnregisterBotResponse
{
    public bool ok;
    public string botId;
    public string trace;
}

[Serializable]
public class DecideActionRequest
{
    public string matchId;
    public string phase;
    public int wave;
    public int cycle;
    public string botId;
    public string botName;
    public string[] infectedPlayers;
    public string[] humanPlayers;
    public string[] alivePlayers;
    public int taskProgress;
    public string nearestHuman;
    public string botRoom;
    public string nearestHumanRoom;
    public float secondsSinceLastSeenHuman;
    public bool isFinalChase;
}

[Serializable]
public class DecideActionResponse
{
    public string botId;
    public string behaviorMode;
    public string targetRoom;
    public string targetPlayer;
    public bool shouldChase;
    public int nextDecisionInSeconds;
    public string trace;
}

[Serializable]
public class ChatMessageDto
{
    public string sender;
    public string senderName;
    public string text;
}

[Serializable]
public class RespondRequest
{
    public string matchId;
    public string phase;
    public int wave;
    public int cycle;
    public string botId;
    public string botName;
    public string personality;
    public string message;
    public ChatMessageDto latestMessage;
    public ChatMessageDto[] recentChat;
    public string[] alivePlayers;
    public string[] humanPlayers;
    public string[] infectedPlayers;
}

[Serializable]
public class RespondResponse
{
    public string botId;
    public bool respond;
    public string[] messages;
    public float typingDelaySeconds;
    public float secondMessageDelaySeconds;
    public string trace;
}

[Serializable]
public class VoteRequest
{
    public string matchId;
    public string phase;
    public int wave;
    public int cycle;
    public string botId;
    public string botName;
    public string[] alivePlayers;
    public string[] humanPlayers;
    public string[] infectedPlayers;
    public ChatMessageDto[] recentChat;
}

[Serializable]
public class VoteResponse
{
    public string botId;
    public string voteTarget;
    public string reason;
    public string trace;
}

[Serializable]
public class TraceEntry
{
    public string ts;
    public string eventType;
    public string matchId;
    public string botId;
    public string input;
    public string output;
    public string trace;
}

[Serializable]
public class TraceResponse
{
    public string matchId;
    public int count;
    public TraceEntry[] traces;
}
