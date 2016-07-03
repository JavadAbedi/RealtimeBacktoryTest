using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using WebSocketSharp;
using WebSocketSharp.Net;

public class StompWebsocketHandler {
    public List<string> deliveryIds = new List<string>();
    private Queue<StompMessage> toSendMessages = new Queue<StompMessage>();
    public StompWebsocket stomp;
    private Dictionary<string, HashSet<Action<string, string>>> dict = new Dictionary<string, HashSet<Action<string, string>>>();
    private Dictionary<string, HashSet<Action<string, string>>> responseDict = new Dictionary<string, HashSet<Action<string, string>>>();
    private string responseDestination;
    private string instanceId;
    public int heartBeatPeriod = 40000; 

	public const string ConnectionLostErrorMessage = "connection_lost";

    public delegate void onConnect();
    public event onConnect OnConnect;
    public delegate void onDisconnect(object sender, EventArgs e);
    public event onDisconnect OnDisconnect;
    public delegate void onWebSocketError(object sender, ErrorEventArgs e);
    public event onWebSocketError OnWebSocketError;
    public delegate void onStompError(string message);
    public event onStompError OnStompError;
    public delegate void onMessageReceived(object sender, MessageEventArgs e);
    public event onMessageReceived OnMessageReceived;

    public bool isAlive { get; private set; }

    public void init(string url, string token, string instanceId, String realtimeChallengeId){
        this.instanceId = instanceId;
        toSendMessages = new Queue<StompMessage>();
		if(UnityThreadHelper.Dispatcher == null) {
			throw new Exception();
		}
		isAlive = false;
        stomp = new StompWebsocket(new WebSocket(url));
		stomp.id = "";
        stomp.webSocket.OnOpen += (object sender, EventArgs e) => {
			if (realtimeChallengeId != null)
                stomp.realtimeConnect(instanceId, realtimeChallengeId);
            else
                stomp.connect(instanceId);
   		};
        stomp.webSocket.SetCookie(new Cookie("Authorization-Bearer", token));
        stomp.webSocket.SetCookie(new Cookie("X-Backtory-Connectivity-Id", instanceId));
        if (realtimeChallengeId != null) {
            stomp.webSocket.SetCookie(new Cookie("X-Backtory-Realtime-Challenge-Id", realtimeChallengeId));
        }

        stomp.webSocket.OnClose += new EventHandler<CloseEventArgs>(onClosed);
        stomp.webSocket.OnMessage += new EventHandler<MessageEventArgs>(onMessage);
        stomp.webSocket.OnError += new EventHandler<ErrorEventArgs>(onWebsocketErrorInternal);
        stomp.webSocket.Connect();
    }
    
    public void matchmakingRequest(string matchmakingName, int skill, String metaData) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/MatchmakingRequest" + "\n\n" +
                "{\"matchmakingName\" : \"" + matchmakingName + "\", " +
                "\"metaData\" : \"" + metaData + "\", " +
                "\"skill\" : " + skill + "}"
                + "\n\0");
    }
    
    public void createChatGroup(string groupName, string type) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/create" + "\n\n" +
                "{\"name\" : \"" + groupName + "\", " +
                "\"type\" : \"" + type + "\"}" + 
                "\n\0");
    }
    
    public void listChatGroups() {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/list" + "\n\n" +
                "\n\0");
    }
    
    public void listChatGroupMembers(string groupId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/listMembers" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\"}"
                + "\n\0");
    }
    
    public void addMemberToChatGroup(string groupId, string userId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/addMember" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"userId\" : \"" + userId + "\"}"
                + "\n\0");
    }
    
    public void removeMemberFromChatGroup(string groupId, string userId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/removeMember" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"userId\" : \"" + userId + "\"}"
                + "\n\0");
    }
    
    public void joinChatGroup(string groupId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/join" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\"}"
                + "\n\0");
    }
    
    public void leaveChatGroup(string groupId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/leave" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\"}"
                + "\n\0");
    }
    
    public void sendChatToGroup(string groupId, string message) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/send" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"message\" : \"" + message + "\"}"
                + "\n\0");
    }
    
    public void sendChatToUser(string userId, string message) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/send" + "\n\n" +
                "{\"userId\" : \"" + userId + "\", " +
                "\"message\" : \"" + message + "\"}"
                + "\n\0");
    }
    
    public void inviteToChatGroup(string groupId, string userId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/inviteUser" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"userId\" : \"" + userId + "\"}"
                + "\n\0");
    }
    
    public void addOwnerToChatGroup(string groupId, string userId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/addOwner" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"userId\" : \"" + userId + "\"}"
                + "\n\0");
    }
    
    public void offlineMessageRequest() {
        String json = fastJSON.JSON.ToJSON(deliveryIds);
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/offline" + "\n\n" +
                "{\"deliveryIdList\" : " + json + "}"
                + "\n\0");
    }
    
    public void sendDeliveryList(List<string> deliveryIdList) {
        String json = fastJSON.JSON.ToJSON(deliveryIdList);
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/deliveryList" + "\n\n" +
                "{\"deliveryIdList\" : " + json + "}"
                + "\n\0");
    }
    
    public void sendDelivery(string deliveryId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/delivery" + "\n\n" +
                "{\"deliveryId\" : \"" + deliveryId + "\"}"
                + "\n\0");
    }
    
    public void directHistory(long lastDate) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/history" + "\n\n" +
                "{\"lastDate\" : " + lastDate + "}"
                + "\n\0");
    }
    
    public void groupHistory(long lastDate, string groupId) {
        stomp.webSocket.Send("SEND\n" +
                "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                "destination:" + "/app/chat/group/history" + "\n\n" +
                "{\"groupId\" : \"" + groupId + "\", " +
                "\"lastDate\" : " + lastDate + "}"
                + "\n\0");
    }
    
    private Timer aliveTimer;
    public void CheckAlive(){
        aliveTimer = new Timer();
        aliveTimer.Elapsed += CheckAliveAction;
        aliveTimer.Interval = heartBeatPeriod; // in miliseconds
        aliveTimer.Start();
    }

    private int wasProblem;
    private const int retryCount = 3;
    private void CheckAliveAction(object sender,EventArgs args){
        stomp.heartbeat();
    }

    private Timer checkHeartFailureTimer;

    public void heartBeaten(){
        if (checkHeartFailureTimer != null){
            checkHeartFailureTimer.Stop();
            checkHeartFailureTimer.Start();
        }
    }

    private void onMessage(object sender, MessageEventArgs e){
        OnMessageReceived(sender, e);
    }
    
    private void handleErrorCommand(string message){
        UnityThreadHelper.Dispatcher.Dispatch(() => {
            if (OnStompError != null) OnStompError(message);
        });
    }

    public StompWebsocketHandler send(StompMessage sMessage){
		Debug.Log(responseDestination);
        toSendMessages.Enqueue(sMessage);
        sendToSendMessages();
        return this;
    }

	public string getPrefix(string destination){
		Match m = Regex.Match(destination, "/.*/");
		if(m != null){
			return m.Value;
		}
		return null;
	}

    private void sendToSendMessages(){
        StompMessage sm;
        while (toSendMessages.Count > 0 && isAlive){
            sm = toSendMessages.Dequeue();
            stomp.send(sm.text,sm.contentType);
        }
    }

    public StompWebsocketHandler() { }

    private void onWebsocketErrorInternal(object sender, ErrorEventArgs e){
        if(OnWebSocketError!=null) UnityThreadHelper.Dispatcher.Dispatch(() => { OnWebSocketError(sender, e); });
    }
    
    private void onClosed(object sender, EventArgs e){
        isAlive = false;
        if (OnDisconnect != null) UnityThreadHelper.Dispatcher.Dispatch(() => { OnDisconnect(sender, e); });
        if(aliveTimer!=null)aliveTimer.Stop();
    }

    public void close(){
        isAlive = false;
        if (stomp != null){
            stomp.disconnect();
            stomp.webSocket.Close();
        }
        if (aliveTimer != null) aliveTimer.Stop();
    }
}
