using System;
using UnityEngine.UI;
using System.Collections.Generic;
public class ConnectToRealtimeServer {
    String token = null;
    String connectivityInstanceId = null;

    public String realtimeChallengeId;
    private String address;
    public Text tConsole;
	RealtimeServerHandler serverHandler;

        public ConnectToRealtimeServer(String address, String connectivityInstanceId, 
            String token, String realtimeChallengeId, Text tConsole) {
        this.address = address;
        this.realtimeChallengeId = realtimeChallengeId;
        this.token = token;
        this.connectivityInstanceId = connectivityInstanceId;
        this.tConsole = tConsole;
    }
    
    public void sendToChallenge(String message) {
        serverHandler.sendToChallenge(message);
    }
    
    public void sendToChallengeChat(string message) {
        serverHandler.sendToChallengeChat(message);
    }
    
    public void sendChallengeResult(List<String> winnersList) {
        serverHandler.sendChallengeResult(winnersList);
    }
    
    public void init() {
        serverHandler = new RealtimeServerHandler(address, connectivityInstanceId, 
            token, realtimeChallengeId);
        serverHandler.InitWebSocket();
        serverHandler.OnMessage += onMessage;
        serverHandler.OnConnect += onConnect;
        serverHandler.OnDisconnect += onDisconnect;
        serverHandler.OnConnected += onConnected;
        serverHandler.OnException += onException;
        serverHandler.OnDirectChatMessage += onDirectChatMessage;
        serverHandler.OnChallengeChatMessage += onChallengeChatMessage;
        serverHandler.OnChallengeJoined += onChallengeJoined;
        serverHandler.OnChallengeStarted += onChallengeStarted;
        serverHandler.OnStartedWebhookMessage += onStartedWebhookMessage;
        serverHandler.OnDirectChatMessage += onDirectChatMessage;
        serverHandler.OnChallengeEventMessage += onChallengeEventMessage;
        serverHandler.OnChallengeChatMessage += onChallengeChatMessage;
        serverHandler.OnResultReceived += onResultReceived;
        serverHandler.OnMasterMessage += onMasterMessage;
        serverHandler.OnWebhookErrorMessage += onWebhookErrorMessage;
        serverHandler.OnJoinedWebhookMessage += onJoinedWebhookMessage;
        serverHandler.OnStartedWebhookMessage += onStartedWebhookMessage;
        serverHandler.OnChallengeEndedMessage += onChallengeEndedMessage;
        serverHandler.OnChallengeDisconnectMessage += onChallengeDisconnectMessage;
    }
    
    public void onMessage(String message) {
        addToConsole("---Message: " + message);
    }
    
    public void onConnect() {
        addToConsole("---Connected");
    }

    public void onDisconnect() {
        addToConsole("---Disconnected");
    }
    
    public void onConnected(String userId) {
		addToConsole("---Connected: " + "userId: " + userId);
	}

    public void onException(String exceptionsString) {
        addToConsole("---Exceptions: " + exceptionsString);
    }

    public void onChallengeJoined(ChallengeJoinedMessage joinedMessage) {
        addToConsole("---user joined: " + joinedMessage.userId + " name: " + joinedMessage.username);
    }
    public void onChallengeStarted(ChallengeStartedMessage startedMessage) {
        addToConsole("---challenge started at: " + startedMessage.startedDate);
    }
    public void onStartedWebhookMessage(StartedWebhookMessage startedMessage) {
        addToConsole("---started webhook message: " + startedMessage.message);
    }
    public void onDirectChatMessage(DirectChatMessage directMessage) {
        addToConsole("---Direct Chat Message: " + directMessage.message + " from: " + directMessage.userId);
    }
    public void onChallengeEventMessage(ChallengeEventMessage challengeEventMessage) {
        
            addToConsole("---Challenge Event: " + challengeEventMessage.message + " from: "
                    + challengeEventMessage.userId);
    }
    public void onChallengeChatMessage(ChallengeChatMessage chatMessage) {
        addToConsole("---Challenge Chat Message: " + chatMessage.message + " from: " + chatMessage.userId);
    }
    public void onResultReceived(BacktoryRealtimeMessage resultReceived) {
        addToConsole("---Your Result Received. Thanks for reporting!!!");
    }
    public void onMasterMessage(MasterMessage masterMessage) {
        addToConsole("---Message from game master: " + masterMessage.message);
    }
    public void onWebhookErrorMessage(WebhookErrorMessage errorMessage) {
        addToConsole("---Webhook error with status: " + errorMessage.httpStatus);
    }
    public void onJoinedWebhookMessage(JoinedWebhookMessage webhookMessage) {
        addToConsole("---Joined webhook message: " + webhookMessage.message);
    }
    public void onChallengeEndedMessage(ChallengeEndedMessage endedMessage) {
        List<String> winnersList = endedMessage.winners;
        String winners = "";
        if (winnersList.Count > 0) {
            winners += winnersList[0];
        }
        for (int i = 1; i < winnersList.Count; i++)
            winners += ", " + winnersList[i];

        addToConsole("---Challenge ended with winners: " + winners);
    }
    public void onChallengeDisconnectMessage(ChallengeDisconnectMessage dcMessage) {
        addToConsole("---User with id " + dcMessage.userId + " disconnected");
    }
    
    void addToConsole(String str) {
		tConsole.text = str + "\n" + tConsole.text;
	}
}
