using System;
using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
public class RealtimeServerHandler {
    private String realtimeChallengeId, connectivityInstanceId, token;
    private String address;
    const string jsonContentType = "application/json";
    public delegate void onConnect();
    public event onConnect OnConnect;
    public delegate void onDisconnect();
    public event onDisconnect OnDisconnect;
    public delegate void onConnected(String userId);
    public event onConnected OnConnected;
    
    public delegate void onException(String exception);
    public event onException OnException;
    public delegate void onDirectChatMessage(DirectChatMessage directMessage);
    public event onDirectChatMessage OnDirectChatMessage;
    public delegate void onChallengeEventMessage(ChallengeEventMessage challengeMessage);
    public event onChallengeEventMessage OnChallengeEventMessage;
    public delegate void onChallengeChatMessage(ChallengeChatMessage challengeMessage);
    public event onChallengeChatMessage OnChallengeChatMessage;
    public delegate void onChallengeJoined(ChallengeJoinedMessage challengeMessage);
    public event onChallengeJoined OnChallengeJoined;
    public delegate void onResultReceived(BacktoryRealtimeMessage backtoryRealtimeMessage);
    public event onResultReceived OnResultReceived;
    public delegate void onMasterMessage(MasterMessage masterMessage);
    public event onMasterMessage OnMasterMessage;
    public delegate void onJoinedWebhookMessage(JoinedWebhookMessage joindMessage);
    public event onJoinedWebhookMessage OnJoinedWebhookMessage;
    public delegate void onChallengeEndedMessage(ChallengeEndedMessage challengeMessage);
    public event onChallengeEndedMessage OnChallengeEndedMessage;
    public delegate void onChallengeDisconnectMessage(ChallengeDisconnectMessage challengeMessage);
    public event onChallengeDisconnectMessage OnChallengeDisconnectMessage;
    public delegate void onWebhookErrorMessage(WebhookErrorMessage webhookMessage);
    public event onWebhookErrorMessage OnWebhookErrorMessage;
    public delegate void onChallengeStarted(ChallengeStartedMessage challengeMessage);
    public event onChallengeStarted OnChallengeStarted;
    public delegate void onStartedWebhookMessage(StartedWebhookMessage startedMessage);
    public event onStartedWebhookMessage OnStartedWebhookMessage;
    public delegate void onMessage(String message);
    public event onMessage OnMessage;
    
    StompWebsocketHandler stompWebsocketHandler = new StompWebsocketHandler();
    protected String generateURI() {
        return "wss://" + address + "ws";
    }

    public RealtimeServerHandler(String address, String connectivityInstanceId, String token,
                             String realtimeChallengeId) {
        this.realtimeChallengeId = realtimeChallengeId;
        this.connectivityInstanceId = connectivityInstanceId;
        this.token = token;
        this.address = address;
		stompWebsocketHandler.OnConnect += OnConnectHandler;
		stompWebsocketHandler.OnDisconnect += OnDisconnectHandler;
		stompWebsocketHandler.OnWebSocketError += OnWebSocketErrorHandler;
		stompWebsocketHandler.OnStompError += OnStompErrorHandler;
		stompWebsocketHandler.OnMessageReceived += OnMessageReceived;
    }
    
    public void InitWebSocket() {                
		stompWebsocketHandler.init(generateURI(), token, connectivityInstanceId, realtimeChallengeId);
	}
    
    public void disconnect() {
      stompWebsocketHandler.close();
	}

    public void sendToChallenge(object jsonObject) {
        stompWebsocketHandler.stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + connectivityInstanceId + "\n" +
                "destination:" + "/app/challenge." + realtimeChallengeId + "\n\n" +
                "{\"message\":" + jsonObject.ToString() + "}" + "\n\0");
    }
    public void sendToChallenge(string message) {
        stompWebsocketHandler.stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + connectivityInstanceId + "\n" +
                "destination:" + "/app/challenge." + realtimeChallengeId + "\n\n" +
                "{\"message\":\"" + message + "\"}" + "\n\0");
	}

    public void sendToChallengeChat(string message) {
        stompWebsocketHandler.stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + connectivityInstanceId + "\n" +
                "destination:" + "/app/challenge." + realtimeChallengeId + "\n\n" +
                "{\"message\":\"" + message + "\"}" + "\n\0");
	}
    
    public void sendChallengeResult(List<String> winnersList) {
        WinnerList winnerList = new WinnerList();
        winnerList.winners = winnersList;
        String json = fastJSON.JSON.ToJSON(winnerList);
        stompWebsocketHandler.stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + connectivityInstanceId + "\n" +
                "destination:" + "/app/challenge.result." + realtimeChallengeId + "\n\n" +
                json.ToString() +
                "\n\0");
    }

    public void sendDirectToUser(String userId, String message) {
        stompWebsocketHandler.stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + connectivityInstanceId + "\n" +
                "X-Backtory-Challenge-Id:" + realtimeChallengeId + "\n" +
                "destination:" + "/app/direct." + userId + "\n\n" +
                "{\"message\":\"" + message + "\"}" + "\n\0");
    }
	
	void OnConnectHandler() {
		Debug.Log("on connect");
		OnConnect();
	}
	
	void OnDisconnectHandler(object sender, EventArgs e) {
		Debug.Log("disconnected");
		OnDisconnect();
	}
	
	void OnWebSocketErrorHandler(object sender, ErrorEventArgs e) {
		if(e.Exception != null) {
			Debug.Log("check your internet connection.");
		} else {
			Debug.Log(e.Message);
		}
		OnDisconnect();
	}

	void OnStompErrorHandler(string message) {
		Debug.Log("OnStompErrorHandler: " + message);
		OnDisconnect();
	}
    
    private void OnMessageReceived(object sender, MessageEventArgs e){
        string message = e.Data;
        UnityThreadHelper.Dispatcher.Dispatch(() => {
            if (message.Equals("")) {
                Debug.Log("--empty web socket message");
                return;
            }
            if (message.Length == 1) {
                Debug.Log("--received empty message");
                return;
            }
            if (message.Length == 2) {
                Debug.Log("--received server dc message");
            }
            
            String[] splitText = message.Replace("\n\n", "\r").Split("\r".ToCharArray());
            String[] headers = splitText[0].Split("\n".ToCharArray());

            if (headers[0].Equals("CONNECTED")) {
                for (int i = 0; i < headers.Length; i++)
                    if (headers[i].StartsWith("heart-beat")) {
                        String heartBeat = headers[i].Split(":".ToCharArray())[1];
                        stompWebsocketHandler.heartBeatPeriod = Int32.Parse(heartBeat.Split(",".ToCharArray())[1]);
                        if (stompWebsocketHandler.heartBeatPeriod != 0)
                            stompWebsocketHandler.CheckAlive();
                        Debug.Log("realtime heart: " + stompWebsocketHandler.heartBeatPeriod);
                    } else if (headers[i].StartsWith("user-id")) {
                        String userId = headers[i].Split(":".ToCharArray())[1];
                        OnConnected(userId);
                    }
				return;
            } else if (headers.Length == 3 && headers[0].Equals("ERROR")) {
                Debug.Log("--ERROR: " + headers[1] + " " + headers[2]);
				return;
            } else if (splitText.Length == 1) {
                Debug.Log("--unknown at realtime: " + message);
				return;
            }
            String body = splitText[1];
            String command = headers[0];
            if (command.Equals("MESSAGE")) {
                body = body.Substring(0, body.Length  - 1);
                BacktoryRealtimeMessage backtoryRealtimeMessage = (BacktoryRealtimeMessage) 
                    fastJSON.JSON.ToObject(body, typeof(BacktoryRealtimeMessage));
                String _class = backtoryRealtimeMessage._type;
                if (_class == null) {
                    Debug.Log(message);
					OnMessage(message);
                } else if (_class.Equals(BacktoryRealtimeMessage.EXCEPTION)) {
                    Debug.Log(BacktoryConnectivityMessage.EXCEPTION);
					OnException(body);	
                } else if (_class.Equals(BacktoryRealtimeMessage.DIRECT)) {
                    Debug.Log(BacktoryRealtimeMessage.DIRECT);
					DirectChatMessage directMessage = (DirectChatMessage) 
						fastJSON.JSON.ToObject(body, typeof(DirectChatMessage));
                    OnDirectChatMessage(directMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHALLENGE)) {
                    Debug.Log(BacktoryRealtimeMessage.CHALLENGE);
					ChallengeEventMessage challengeMessage = (ChallengeEventMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeEventMessage));
                    OnChallengeEventMessage(challengeMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHAT)) {
                    Debug.Log(BacktoryRealtimeMessage.CHAT);
					ChallengeChatMessage chatMessage = (ChallengeChatMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeChatMessage));
                    OnChallengeChatMessage(chatMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHALLENGE_JOINED)) {
                    Debug.Log(BacktoryRealtimeMessage.CHALLENGE_JOINED);
					ChallengeJoinedMessage challengeMessage = (ChallengeJoinedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeJoinedMessage));
                    OnChallengeJoined(challengeMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.RESPONSE_RECEIVED)) {
                    Debug.Log(BacktoryRealtimeMessage.RESPONSE_RECEIVED);
					BacktoryRealtimeMessage responseMessage = (BacktoryRealtimeMessage) 
						fastJSON.JSON.ToObject(body, typeof(BacktoryRealtimeMessage));
                    OnResultReceived(responseMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.MASTER)) {
                    Debug.Log(BacktoryRealtimeMessage.MASTER);
					MasterMessage masterMessage = (MasterMessage) 
						fastJSON.JSON.ToObject(body, typeof(MasterMessage));
                    OnMasterMessage(masterMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.JOINED_WEBHOOK)) {
                    Debug.Log(BacktoryRealtimeMessage.JOINED_WEBHOOK);
					JoinedWebhookMessage joindMessage = (JoinedWebhookMessage) 
						fastJSON.JSON.ToObject(body, typeof(JoinedWebhookMessage));
                    OnJoinedWebhookMessage(joindMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHALLENGE_ENDED)) {
                    Debug.Log(BacktoryRealtimeMessage.CHALLENGE_ENDED);
					ChallengeEndedMessage challengeMessage = (ChallengeEndedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeEndedMessage));
                    OnChallengeEndedMessage(challengeMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHALLENGE_DISCONNECT)) {
                    Debug.Log(BacktoryRealtimeMessage.CHALLENGE_DISCONNECT);
					ChallengeDisconnectMessage challengeMessage = (ChallengeDisconnectMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeDisconnectMessage));
                    OnChallengeDisconnectMessage(challengeMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.WEBHOOK_ERROR)) {
                    Debug.Log(BacktoryRealtimeMessage.WEBHOOK_ERROR);
					WebhookErrorMessage webhookMessage = (WebhookErrorMessage) 
						fastJSON.JSON.ToObject(body, typeof(WebhookErrorMessage));
                    OnWebhookErrorMessage(webhookMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.CHALLENGE_STARTED)) {
                    Debug.Log(BacktoryRealtimeMessage.CHALLENGE_STARTED);
					ChallengeStartedMessage challengeMessage = (ChallengeStartedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChallengeStartedMessage));
                    OnChallengeStarted(challengeMessage);
                } else if (_class.Equals(BacktoryRealtimeMessage.STARTED_WEBHOOK)) {
                    Debug.Log(BacktoryRealtimeMessage.STARTED_WEBHOOK);
					StartedWebhookMessage startedMessage = (StartedWebhookMessage) 
						fastJSON.JSON.ToObject(body, typeof(StartedWebhookMessage));
                    OnStartedWebhookMessage(startedMessage);
                } else {
                    Debug.Log("--received message: " + body);
                    OnMessage(body);
                }
            } else {
                Debug.Log("--received message: " + body);
            }
        });
    }
}
