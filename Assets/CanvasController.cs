using UnityEngine;
using UnityEngine.UI;
using System;

public class CanvasController : MonoBehaviour {
	public static string AUTHENTICATION_ID = "5738186ae4b0179857153b63";
	public static string AUTHENTICATION_KEY = "5738186ae4b09a52993fe413";
	public static string CONNECTIVITY_ID = "573818b7e4b0179857153b72";
	public Button bConnect, bMatchMaking, bDisconnect, bClear, bGameMessage, bChat;
	public Text tConsole;
	public InputField etUsername, etPassword, etMessage;
	string access_token = null;
	ConnectivityServerHandler serverHandler = new ConnectivityServerHandler(CONNECTIVITY_ID);
	ConnectToRealtimeServer connectToRealtimeServer;
	Authentication authentication = (new Authentication(AUTHENTICATION_ID, AUTHENTICATION_KEY));
	
	// Use this for initialization
	void Start () {
		bConnect.onClick.AddListener(() => {
			addToConsole("connecting ...");
			authentication.login(etUsername.text, etPassword.text, (Login login) => {
				if (login.access_token != null) {
					addToConsole("connected");
					access_token = login.access_token;
					serverHandler.InitWebSocket(access_token);
				}
			});
		});
		
		bClear.onClick.AddListener(() => {
			tConsole.text = "";
		});
		
		bDisconnect.onClick.AddListener(() => {
			serverHandler.disconnect();
		});
		
		bMatchMaking.onClick.AddListener(() => {
			serverHandler.matchmakingRequest("matchmaking1", 55);
		});
		
		bGameMessage.onClick.AddListener(() => {
			connectToRealtimeServer.sendToChallenge(etMessage.text);
		});
		
		bChat.onClick.AddListener(() => {
			connectToRealtimeServer.sendToChallengeChat(etMessage.text);
		});
				
		addListenerToServerHandler();
	}
	
	private void addListenerToServerHandler() {
		serverHandler.OnMatchFound += onMatchFound;
		serverHandler.OnMatchResponse += onMatchResponse;
		serverHandler.OnMatchCancellationResponse += onMatchCancellationResponse;
		serverHandler.OnOpen += onOpen;
		serverHandler.OnMessage += onMessage;
		serverHandler.OnException += onException;
		serverHandler.OnError += onError;
		serverHandler.OnClose += onClose;
		serverHandler.OnMatchNotFound += onMatchNotFound;
		serverHandler.OnMatchUpdate += onMatchUpdate;
		serverHandler.OnChatMessage += onChatMessage;
		serverHandler.OnPushMessage += onPushMessage;
	}
	
	public void onMatchFound(MatchFoundMessage matchFoundMessage) {
		addToConsole("---Matchmaking with id " +
                        matchFoundMessage.requestId +
                        " and challenge id " +
                        matchFoundMessage.realtimeChallengeId + " found");
		connectToRealtimeServer = new ConnectToRealtimeServer(matchFoundMessage.address,
			CONNECTIVITY_ID, access_token, matchFoundMessage.realtimeChallengeId, tConsole);
		connectToRealtimeServer.init();
	}

	public void onMatchResponse(String requestId) {
		addToConsole("---Your matchmaking request received with id: " + requestId);
	}
	
	public void onMatchCancellationResponse(String requestId) {
		addToConsole("---Your matchmaking request cancel with id: " + requestId);
	}
	
	public void onOpen() {
		addToConsole("---Connected");
	}

	public void onMessage(String message) {
		addToConsole("---Message: " + message);
	}

	public void onException(String exceptionsString) {
		addToConsole("---Exceptions: " + exceptionsString);
	}
	
	public void onError(String message) {
		addToConsole("---Error");
	}

	public void onClose() {
		addToConsole("---Closed");
	}

	public void onMatchNotFound(MatchNotFoundMessage matchNotFoundMessage) {
		addToConsole("---Matchmaking with id " +
				matchNotFoundMessage.requestId + " not found");
	}
	
	public void onMatchUpdate(MatchUpdateMessage matchUpdateMessage) {
		addToConsole("---Matchmaking with id "
				+ matchUpdateMessage.requestId + " updated");
	}
	
	public void onChatMessage(ChatMessage chatMessage) {
		addToConsole("---Chat message: " + chatMessage.message + " from: " + chatMessage.senderId
				+ " at: " + chatMessage.date);
	}

	public void onPushMessage(PushNotifMessage pushNotifMessage) {
		addToConsole("---Push message: " + pushNotifMessage.message);
	}

	// Update is called once per frame
	void Update () {		
	}
	
	void addToConsole(String str) {
		tConsole.text = str + "\n" + tConsole.text;
	}
}
