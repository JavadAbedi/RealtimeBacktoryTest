using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
public class ConnectivityServerHandler {
	const string BACKTORY_WS_URL = "wss://ws.backtory.com/connectivity/ws";
    // const string BACKTORY_WS_URL = "ws://192.168.0.107:8090/ws";
    
    public static string GROUP_TYPE_PUBLIC = "Public";
    public static string GROUP_TYPE_PRIVATE = "Private";
    public delegate void onConnect();
    public event onConnect OnConnect;
    public delegate void onDisconnect();
    public event onDisconnect OnDisconnect;
    public delegate void onConnected(String userId);
    public event onConnected OnConnected;

    public delegate void onMatchFound(MatchFoundMessage matchFoundMessage);
    public event onMatchFound OnMatchFound;
    public delegate void onMatchResponse(String requestId);
    public event onMatchResponse OnMatchResponse;
    public delegate void onMatchCancellationResponse(String requestId);
    public event onMatchCancellationResponse OnMatchCancellationResponse;
    public delegate void onOpen();
    public event onOpen OnOpen;
    public delegate void onMessage(String message);
    public event onMessage OnMessage;
    public delegate void onException(String exceptionsString);
    public event onException OnException;
    public delegate void onError(String message);
    public event onError OnError;
    public delegate void onClose();
    public event onClose OnClose;
    public delegate void onMatchNotFound(MatchNotFoundMessage matchNotFoundMessage);
    public event onMatchNotFound OnMatchNotFound;
    public delegate void onMatchUpdate(MatchUpdateMessage matchUpdateMessage);
    public event onMatchUpdate OnMatchUpdate;
    public delegate void onChatMessage(SimpleChatMessage chatMessage);
    public event onChatMessage OnChatMessage;
    public delegate void onPushMessage(SimpleChatMessage simpleMessage);
    public event onPushMessage OnPushMessage;

    public delegate void onChatGroupCreatedMessage(ChatGroupCreatedMessage createdMessage);
    public event onChatGroupCreatedMessage OnChatGroupCreatedMessage;
    public delegate void onUserChatHistoryMessage(UserChatHistoryMessage historyMessage);
    public event onUserChatHistoryMessage OnUserChatHistoryMessage;
    public delegate void onGroupChatHistoryMessage(GroupChatHistoryMessage historyMessage);
    public event onGroupChatHistoryMessage OnGroupChatHistoryMessage;
    public delegate void onGroupPushMessage(SimpleChatMessage chatMessage);
    public event onGroupPushMessage OnGroupPushMessage;
    public delegate void onGroupChatMessage(SimpleChatMessage chatMessage);
    public event onGroupChatMessage OnGroupChatMessage;
    public delegate void onChatGroupMembersListMessage(ChatGroupMembersListMessage groupMembersList);
    public event onChatGroupMembersListMessage OnChatGroupMembersListMessage;
    public delegate void onChatGroupsListMessage(ChatGroupsListMessage groupListMessage);
    public event onChatGroupsListMessage OnChatGroupsListMessage;
    public delegate void onChatGroupOwnerAddedMessage(ChatGroupOwnerAddedMessage groupOwnerAddedMessage);
    public event onChatGroupOwnerAddedMessage OnChatGroupOwnerAddedMessage;
    public delegate void onChatGroupLeftMessage(ChatGroupLeftMessage leftMessage);
    public event onChatGroupLeftMessage OnChatGroupLeftMessage;
    public delegate void onChatGroupInvitedMessage(ChatGroupInvitedMessage invitedMessage);
    public event onChatGroupInvitedMessage OnChatGroupInvitedMessage;
    public delegate void onChatGroupUserRemovedMessage(UserRemovedMessage userRemovedMessage);
    public event onChatGroupUserRemovedMessage OnChatGroupUserRemovedMessage;
    public delegate void onChatGroupUserLeftMessage(UserLeftMessage userLeftMessage);
    public event onChatGroupUserLeftMessage OnChatGroupUserLeftMessage;
    public delegate void onChatGroupUserJoinedMessage(UserJoinedMessage userJoinedMessage);
    public event onChatGroupUserJoinedMessage OnChatGroupUserJoinedMessage;
    public delegate void onChatGroupUserAddedMessage(UserAddedMessage userAddedMessage);
    public event onChatGroupUserAddedMessage OnChatGroupUserAddedMessage;
    public delegate void onChatInvitationMessage(ChatInvitationMessage invitationMessage);
    public event onChatInvitationMessage OnChatInvitationMessage;
    public delegate void onEmptyOffline();
    public event onEmptyOffline OnEmptyOffline;

	string connectivityId;
	private List<string> requestList = new List<string>();
	StompWebsocketHandler stompWebsocketHandler = new StompWebsocketHandler();

	public ConnectivityServerHandler(string connectivityId) {
		this.connectivityId = connectivityId;
		stompWebsocketHandler.OnConnect += OnConnectHandler;
		stompWebsocketHandler.OnDisconnect += OnDisconnectHandler;
		stompWebsocketHandler.OnWebSocketError += OnWebSocketErrorHandler;
		stompWebsocketHandler.OnStompError += OnStompErrorHandler;
		stompWebsocketHandler.OnMessageReceived += OnMessageReceived;
	}
	
	private void OnMessageReceived(object sender, MessageEventArgs e){
        string message = e.Data;
        UnityThreadHelper.Dispatcher.Dispatch(() => {
            if (message.Equals("")) {
                Debug.Log("--empty web socket message. maybe ping response");
                return;
            } else if (e.RawData.Length == 1){
                stompWebsocketHandler.heartBeaten();
            } else if (e.RawData.Length == 2){
                Debug.Log("--received server dc message");
                return;
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
                        Debug.Log("connectivity heart: " + stompWebsocketHandler.heartBeatPeriod);
                    } else if (headers[i].StartsWith("user-id")) {
                        String userId = headers[i].Split(":".ToCharArray())[1];
                        OnConnected(userId);
                    }
				return;
            } else if (headers.Length == 3 && headers[0].Equals("ERROR")) {
                Debug.Log("--ERROR: " + headers[1] + " " + headers[2]);
				return;
            } else if (splitText.Length == 1) {
                // Debug.Log("--unknown at main: " + message);
				return;
            }
            String body = splitText[1];
            String command = headers[0];
            if (command.Equals("MESSAGE")) {
                foreach (String header in headers) {
                    String[] splitedHeader = header.Split(":".ToCharArray());
                    if (splitedHeader[0].Equals("delivery-id")) {
                        stompWebsocketHandler.stomp.webSocket.Send("ACK\n" +
                                "X-Backtory-Connectivity-Id:" + connectivityId + "\n" +
                                "delivery-id:" + splitedHeader[1] + "\n" +
                                "\n\0");
                    }
                }
                
                body = body.Substring(0, body.Length  - 1);
                BacktoryRealtimeMessage backtoryRealtimeMessage = (BacktoryRealtimeMessage) 
                    fastJSON.JSON.ToObject(body, typeof(BacktoryRealtimeMessage));
                String _class = backtoryRealtimeMessage._type;
                if (_class == null) {
                    Debug.Log(message);
					OnMessage(message);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_CREATED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_CREATED_MESSAGE);
					ChatGroupCreatedMessage chatMessage = (ChatGroupCreatedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupCreatedMessage));
					OnChatGroupCreatedMessage(chatMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_MEMBERS_LIST_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_MEMBERS_LIST_MESSAGE);
					ChatGroupMembersListMessage listMessage = (ChatGroupMembersListMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupMembersListMessage));
					OnChatGroupMembersListMessage(listMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUPS_LIST_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUPS_LIST_MESSAGE);
					ChatGroupsListMessage chatMessage = (ChatGroupsListMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupsListMessage));
					OnChatGroupsListMessage(chatMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_OWNER_ADDED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_OWNER_ADDED_MESSAGE);
					ChatGroupOwnerAddedMessage addedMessage = (ChatGroupOwnerAddedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupOwnerAddedMessage));
					OnChatGroupOwnerAddedMessage(addedMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_LEFT_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_LEFT_MESSAGE);
					ChatGroupLeftMessage leftMessage = (ChatGroupLeftMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupLeftMessage));
					OnChatGroupLeftMessage(leftMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_INVITED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_INVITED_MESSAGE);
					ChatGroupInvitedMessage invitedMessage = (ChatGroupInvitedMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatGroupInvitedMessage));
					OnChatGroupInvitedMessage(invitedMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_REMOVED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_USER_REMOVED_MESSAGE);
					UserRemovedMessage removedMessage = (UserRemovedMessage) 
						fastJSON.JSON.ToObject(body, typeof(UserRemovedMessage));
					OnChatGroupUserRemovedMessage(removedMessage);
                    sendDelivery(removedMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_ADDED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_USER_ADDED_MESSAGE);
					UserAddedMessage addedMessage = (UserAddedMessage) 
						fastJSON.JSON.ToObject(body, typeof(UserAddedMessage));
					OnChatGroupUserAddedMessage(addedMessage);
                    sendDelivery(addedMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_JOINED_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_USER_JOINED_MESSAGE);
					UserJoinedMessage joinedMessage = (UserJoinedMessage) 
						fastJSON.JSON.ToObject(body, typeof(UserJoinedMessage));
					OnChatGroupUserJoinedMessage(joinedMessage);
                    sendDelivery(joinedMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_LEFT_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_USER_LEFT_MESSAGE);
					UserLeftMessage leftMessage = (UserLeftMessage) 
						fastJSON.JSON.ToObject(body, typeof(UserLeftMessage));
					OnChatGroupUserLeftMessage(leftMessage);
                    sendDelivery(leftMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.CHAT_GROUP_INVITATION_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.CHAT_GROUP_INVITATION_MESSAGE);
					ChatInvitationMessage invitationMessage = (ChatInvitationMessage) 
						fastJSON.JSON.ToObject(body, typeof(ChatInvitationMessage));
					OnChatInvitationMessage(invitationMessage);
                    sendDelivery(invitationMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE);
					SimpleChatMessage simpleMessage = (SimpleChatMessage) 
						fastJSON.JSON.ToObject(body, typeof(SimpleChatMessage));
                    onSimpleMessage(simpleMessage);
                    sendDelivery(simpleMessage.deliveryId);
                } else if (_class.Equals(BacktoryConnectivityMessage.USER_CHAT_HISTORY_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.USER_CHAT_HISTORY_MESSAGE);
					UserChatHistoryMessage historyMessage = (UserChatHistoryMessage) 
						fastJSON.JSON.ToObject(body, typeof(UserChatHistoryMessage));
					OnUserChatHistoryMessage(historyMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.GROUP_CHAT_HISTORY_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.GROUP_CHAT_HISTORY_MESSAGE);
					GroupChatHistoryMessage historyMessage = (GroupChatHistoryMessage) 
						fastJSON.JSON.ToObject(body, typeof(GroupChatHistoryMessage));
					OnGroupChatHistoryMessage(historyMessage);
                } else if (_class.Equals(BacktoryConnectivityMessage.OFFLINE_CHAT_MESSAGE_LIST)) {
                    Debug.Log(BacktoryConnectivityMessage.OFFLINE_CHAT_MESSAGE_LIST);
					OfflineChatMessageList offlineMessageList = (OfflineChatMessageList) 
						fastJSON.JSON.ToObject(body, typeof(OfflineChatMessageList));
					if (offlineMessageList.messageList.Count == 0) {
                        OnEmptyOffline();
                        return;
                    }
                    for (int i = 0; i < offlineMessageList.messageList.Count; i++) {
                        ComprehensiveChatMessaege innerMessage = offlineMessageList.messageList[i];
                        // Debug.Log("deliveryId: " + innerMessage.deliveryId);
                        stompWebsocketHandler.deliveryIds.Add(innerMessage.deliveryId);
                        // Debug.Log("ids: " + stompWebsocketHandler.deliveryIds);
                        if (innerMessage._type.Equals(BacktoryConnectivityMessage.CHAT_GROUP_INVITATION_MESSAGE)) {
                            ChatInvitationMessage invitationMessage = (ChatInvitationMessage) fastJSON.JSON.ToObject(fastJSON.JSON.ToJSON(innerMessage), typeof(ChatInvitationMessage));
                            OnChatInvitationMessage(invitationMessage);
                        } else if (innerMessage._type.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_REMOVED_MESSAGE)) {
                            UserRemovedMessage removeMessage = (UserRemovedMessage) fastJSON.JSON.ToObject
                            (fastJSON.JSON.ToJSON(innerMessage), typeof(UserRemovedMessage));
                            OnChatGroupUserRemovedMessage(removeMessage);
                        } else if (innerMessage._type.Equals(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE)) {
                            SimpleChatMessage simpleMessage = (SimpleChatMessage) fastJSON.JSON.ToObject
                            (fastJSON.JSON.ToJSON(innerMessage) , typeof(SimpleChatMessage));
                            onSimpleMessage(simpleMessage);
                        } else if (innerMessage._type.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_ADDED_MESSAGE)) {
                            UserAddedMessage addedMessage = (UserAddedMessage) fastJSON.JSON.ToObject
                            (fastJSON.JSON.ToJSON(innerMessage), typeof(UserAddedMessage));
                            OnChatGroupUserAddedMessage(addedMessage);
                        } else if (innerMessage._type.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_JOINED_MESSAGE)) {
                            UserJoinedMessage joinedMessage = (UserJoinedMessage) fastJSON.JSON.ToObject
                            (fastJSON.JSON.ToJSON(innerMessage), typeof(UserJoinedMessage));
                            OnChatGroupUserJoinedMessage(joinedMessage);
                        } else if (innerMessage._type.Equals(BacktoryConnectivityMessage.CHAT_GROUP_USER_LEFT_MESSAGE)) {
                            UserLeftMessage leftMessage = (UserLeftMessage) fastJSON.JSON.ToObject
                            (fastJSON.JSON.ToJSON(innerMessage), typeof(UserLeftMessage));
                            OnChatGroupUserLeftMessage(leftMessage);
                        }
                    }
                }
                
                else if (_class.Equals(BacktoryConnectivityMessage.EXCEPTION)) {
                    Debug.Log(BacktoryConnectivityMessage.EXCEPTION);
					OnException(body);	
                } else if (_class.Equals(BacktoryConnectivityMessage.MATCHMAKING_CANCELLATION_RESPONSE)) {
                    Debug.Log(BacktoryConnectivityMessage.MATCHMAKING_CANCELLATION_RESPONSE);
                	MatchmakingCancellationResponseMessage onMatchCancellationResponse = (MatchmakingCancellationResponseMessage) 
						fastJSON.JSON.ToObject(body, typeof(MatchmakingCancellationResponseMessage));
					requestList.Remove(onMatchCancellationResponse.requestId);
                	OnMatchCancellationResponse(onMatchCancellationResponse.requestId);
				} else if (_class.Equals(BacktoryConnectivityMessage.MATCHMAKING_RESPONSE)) {
                    Debug.Log(BacktoryConnectivityMessage.MATCHMAKING_RESPONSE);
				    MatchmakingResponseMessage matchMakingResponse = (MatchmakingResponseMessage) 
						fastJSON.JSON.ToObject(body, typeof(MatchmakingResponseMessage));
					requestList.Add(matchMakingResponse.requestId);
					if (OnMatchResponse != null) {
						OnMatchResponse(matchMakingResponse.requestId);
					} else {
						Debug.Log("ghalat ast");
					}
                } else if (_class.Equals(BacktoryConnectivityMessage.MATCH_FOUND_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.MATCH_FOUND_MESSAGE);
					MatchFoundMessage matchFoundMessage = (MatchFoundMessage) 
						fastJSON.JSON.ToObject(body, typeof(MatchFoundMessage));
					if (requestList.Contains(matchFoundMessage.requestId)) {
						OnMatchFound(matchFoundMessage);
					} else {
						Debug.Log("--Matchmaking for another device of user");
					}
                } else if (_class.Equals(BacktoryConnectivityMessage.MATCH_NOT_FOUND_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.MATCH_NOT_FOUND_MESSAGE);
					MatchNotFoundMessage matchNotFoundMessage = (MatchNotFoundMessage) 
						fastJSON.JSON.ToObject(body, typeof(MatchNotFoundMessage));
					if (requestList.Contains(matchNotFoundMessage.requestId)) {
						OnMatchNotFound(matchNotFoundMessage);
					} else {
						Debug.Log("--Matchmaking not found for another device of user");
					}
                } else if (_class.Equals(BacktoryConnectivityMessage.MATCH_UPDATE_MESSAGE)) {
                    Debug.Log(BacktoryConnectivityMessage.MATCH_UPDATE_MESSAGE);
					MatchUpdateMessage matchUpdateMessage = (MatchUpdateMessage) 
						fastJSON.JSON.ToObject(body, typeof(MatchUpdateMessage));
					OnMatchUpdate(matchUpdateMessage);
                } else {
					OnMessage(body);
                }
            } else {
                Debug.Log("--received message: " + body);
            }
        });
    }
    
    private void onSimpleMessage(SimpleChatMessage simpleMessage) {
        Debug.Log(simpleMessage.groupId + " " + simpleMessage.senderId + " " + simpleMessage.message);
         if (simpleMessage.groupId == null) {
            if (simpleMessage.senderId == null) {
                OnPushMessage(simpleMessage);
            } else {
                OnChatMessage(simpleMessage);
            }
        } else {
            if (simpleMessage.senderId == null) {
                OnGroupPushMessage(simpleMessage);
            } else {
                OnGroupChatMessage(simpleMessage);
            }
        }
    }

	public void InitWebSocket(string token) {
		stompWebsocketHandler.init(BACKTORY_WS_URL, token, connectivityId, null);
	}
	
	public void matchmakingRequest(string matchmakingName, int skill, String metaData) {
		stompWebsocketHandler.matchmakingRequest(matchmakingName, skill, metaData);
	}
	public void disconnect() {
		stompWebsocketHandler.close();
	}
    
    public void createGroupChat(string groupName, string type) {
        stompWebsocketHandler.createChatGroup(groupName, type);
    }
    
    public void listChatGroups() {
        stompWebsocketHandler.listChatGroups();
    }
    
    public void listChatGroupMembers(string groupId) {
        stompWebsocketHandler.listChatGroupMembers(groupId);
    }
    
    public void addMemberToChatGroup(string groupId, string userId) {
        stompWebsocketHandler.addMemberToChatGroup(groupId, userId);
    }
    
    public void removeMemberFromChatGroup(string groupId, string userId) {
        stompWebsocketHandler.removeMemberFromChatGroup(groupId, userId);
    }
    
    public void joinChatGroup(string groupId) {
        stompWebsocketHandler.joinChatGroup(groupId);
    }
    
    public void leaveChatGroup(string groupId) {
        stompWebsocketHandler.leaveChatGroup(groupId);
    }
    
    public void sendChatToGroup(string groupId, string message) {
        stompWebsocketHandler.sendChatToGroup(groupId, message);
    }
    
    public void sendChatToUser(string userId, string message) {
        stompWebsocketHandler.sendChatToUser(userId, message);
    }
    
    public void inviteToChatGroup(string groupId, string userId) {
        stompWebsocketHandler.inviteToChatGroup(groupId, userId);
    }
    
    public void addOwnerToChatGroup(string groupId, string userId) {
        stompWebsocketHandler.addOwnerToChatGroup(groupId, userId);
    }
    
    public void offlineMessageRequest() {
        stompWebsocketHandler.offlineMessageRequest();
    }
    
    public void directHistory(long lastDate) {
        stompWebsocketHandler.directHistory(lastDate);
    }
    
    public void groupHistory(long lastDate, string groupId) {
        stompWebsocketHandler.groupHistory(lastDate, groupId);
    }
    public void sendDelivery(string deliveryId) {
        stompWebsocketHandler.sendDelivery(deliveryId);
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
			Debug.Log("check your internet connection. " + e.Exception);
		} else {
			Debug.Log(e.Message);
		}
		OnDisconnect();
	}

	void OnStompErrorHandler(string message) {
		Debug.Log("OnStompErrorHandler: " + message);
		OnDisconnect();
	}
}
