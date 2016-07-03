using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CanvasController : MonoBehaviour {
	// backtory
	public static string AUTHENTICATION_ID = "5738186ae4b0179857153b63";
	public static string AUTHENTICATION_KEY = "5738186ae4b09a52993fe413";
	public static string CONNECTIVITY_ID = "573818b7e4b0179857153b72";
	
	// local
	// public static string AUTHENTICATION_ID = "57163ee5e4b0cad8c4dd1844";
	// public static string AUTHENTICATION_KEY = "57163ee5e4b093ed2821a011";
	// public static string CONNECTIVITY_ID = "57284ac1e4b01c017afb4015";
	public Button bLogin, bRegister, bConnect, bMatchMaking, bDisconnect, bClear, bGameMessage, bChat,
		bCreate, bGroupList, bUserList, bAdd, bRemove, bJoin, bLeave, bSendToGroup, bSendToUser, 
		bInvite, bAddOwner, bOffline, bHistoryGroup, bHistoryDirect;
	public Text tConsole;
	public InputField etUsername, etPassword, etFirstName, etLastName, etMessage, etGroupId, etUserId;
	string access_token = null;
	ConnectivityServerHandler serverHandler = new ConnectivityServerHandler(CONNECTIVITY_ID);
	ConnectToRealtimeServer connectToRealtimeServer;
	Authentication authentication = (new Authentication(AUTHENTICATION_ID, AUTHENTICATION_KEY));
	
	// Use this for initialization
	void Start () {
		bConnect.onClick.AddListener(() => {
			addToConsole("connecting ...");
			if (access_token != null) {
				serverHandler.InitWebSocket(access_token);
			} else {
				addToConsole("login first please.");
			}
		});
		
		bLogin.onClick.AddListener(() => {
			authentication.login(etUsername.text, etPassword.text, (LoginResponse login) => {
				if (login.access_token != null) {
					addToConsole("logged in");
					access_token = login.access_token;
				}
			});
		});
		
		bRegister.onClick.AddListener(() => {
			authentication.register(etUsername.text, etPassword.text, 
				etFirstName.text, etLastName.text, (RegisterResponse registerResponse) => {
				addToConsole("signed up");
			});
		});
		
		bClear.onClick.AddListener(() => {
			tConsole.text = "";
		});
		
		bDisconnect.onClick.AddListener(() => {
			serverHandler.disconnect();
		});
		
		bMatchMaking.onClick.AddListener(() => {
			serverHandler.matchmakingRequest("matchmaking1", 55, "sample meta data");
		});
		
		bGameMessage.onClick.AddListener(() => {
			connectToRealtimeServer.sendToChallenge(etMessage.text);
		});
		
		bChat.onClick.AddListener(() => {
			connectToRealtimeServer.sendToChallengeChat(etMessage.text);
		});
		
		bCreate.onClick.AddListener(() => {
			serverHandler.createGroupChat(etGroupId.text, ConnectivityServerHandler.GROUP_TYPE_PUBLIC);
		});
		
		bGroupList.onClick.AddListener(() => {
			serverHandler.listChatGroups();
		});
		
		bUserList.onClick.AddListener(() => {
			serverHandler.listChatGroupMembers(etGroupId.text);
		});
		
		bAdd.onClick.AddListener(() => {
			serverHandler.addMemberToChatGroup(etGroupId.text, etUserId.text);
		});
		
		bRemove.onClick.AddListener(() => {
			serverHandler.removeMemberFromChatGroup(etGroupId.text, etUserId.text);
		});
		
		bJoin.onClick.AddListener(() => {
			serverHandler.joinChatGroup(etGroupId.text);
		});
		
		bLeave.onClick.AddListener(() => {
			serverHandler.leaveChatGroup(etGroupId.text);
		});
		
		bSendToGroup.onClick.AddListener(() => {
			serverHandler.sendChatToGroup(etGroupId.text, etMessage.text);
		});
		
		bSendToUser.onClick.AddListener(() => {
			serverHandler.sendChatToUser(etUserId.text, etMessage.text);
		});
		
		bInvite.onClick.AddListener(() => {
			serverHandler.inviteToChatGroup(etGroupId.text, etUserId.text);
		});
		
		bAddOwner.onClick.AddListener(() => {
			serverHandler.addOwnerToChatGroup(etGroupId.text, etUserId.text);
		});
		
		bOffline.onClick.AddListener(() => {
			serverHandler.offlineMessageRequest();
		});
		
		bHistoryGroup.onClick.AddListener(() => {
			serverHandler.groupHistory(new DateTime().Millisecond, etGroupId.text);
		});
		
		bHistoryDirect.onClick.AddListener(() => {
			serverHandler.directHistory(new DateTime().Millisecond);
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
		serverHandler.OnConnected += onConnected;
		serverHandler.OnDisconnect += onDisconnect;
		
		// Chat
		serverHandler.OnChatGroupCreatedMessage += onChatGroupCreatedMessage;
		serverHandler.OnChatGroupsListMessage += onChatGroupsListMessage;
		serverHandler.OnUserChatHistoryMessage += onUserChatHistoryMessage;
		serverHandler.OnGroupChatHistoryMessage += onGroupChatHistoryMessage;
		serverHandler.OnGroupPushMessage += onGroupPushMessage;
		serverHandler.OnGroupChatMessage += onGroupChatMessage;
		serverHandler.OnChatGroupMembersListMessage += onChatGroupMembersListMessage;
		serverHandler.OnChatGroupOwnerAddedMessage += onChatGroupOwnerAddedMessage;
		serverHandler.OnChatGroupLeftMessage += onChatGroupLeftMessage;
		serverHandler.OnChatGroupInvitedMessage += onChatGroupInvitedMessage;
		serverHandler.OnChatGroupUserRemovedMessage += onChatGroupUserRemovedMessage;
		serverHandler.OnChatGroupUserLeftMessage += onChatGroupUserLeftMessage;
		serverHandler.OnChatGroupUserJoinedMessage += onChatGroupUserJoinedMessage;
		serverHandler.OnChatGroupUserAddedMessage += onChatGroupUserAddedMessage;
		serverHandler.OnChatInvitationMessage += onChatInvitationMessage;
		serverHandler.OnOfflineChatMessage += onOfflineChatMessage;
	}
	
	public void onChatGroupCreatedMessage(ChatGroupCreatedMessage createdMessage) {
		addToConsole("Created message: " + createdMessage.groupId);
	}
	
	public void onUserChatHistoryMessage(List<ChatMessage> messageList) {
		addToConsole("User chat history message received: " + messageList.Count);
		for (int i = 0; i < messageList.Count; i++) {
			addToConsole("type: " + messageList[i]._type);
			if (messageList[i]._type.Equals(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE)) {
				addToConsole("message: " + ((SimpleChatMessage) messageList[i]).message);
			}
		}
	}
	
	public void onGroupChatHistoryMessage(List<ChatMessage> messageList, string groupId) {
		addToConsole("Group chat history message received: " + messageList.Count + " " + groupId);
		for (int i = 0; i < messageList.Count; i++) {
			addToConsole("type: " + messageList[i]._type);
			if (messageList[i]._type.Equals(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE)) {
				addToConsole("message: " + ((SimpleChatMessage) messageList[i]).message);
			}
		}
	}
    
	public void onGroupPushMessage(SimpleChatMessage chatMessage) {
		addToConsole("Group push message: " + chatMessage.groupId + " " 
			+ chatMessage.message);
	}
	
	public void onGroupChatMessage(SimpleChatMessage chatMessage) {
		addToConsole("Group chat message: " + chatMessage.groupId + " " + chatMessage.senderId + " "
			+ chatMessage.message);
	}
	
	public void onChatGroupMembersListMessage(ChatGroupMembersListMessage groupMembersList) {
		addToConsole("Group members list message: " + groupMembersList.groupMemberList.Count);
	}
	
	public void onChatGroupsListMessage(ChatGroupsListMessage groupListMessage) {
		addToConsole("Chat groups list message: " + groupListMessage.groupIdList.Count);
	}
	
	public void onChatGroupOwnerAddedMessage(ChatGroupOwnerAddedMessage groupOwnerAddedMessage) {
		addToConsole("Chat group owner added message: " + groupOwnerAddedMessage.groupId);
	}
	
	public void onChatGroupLeftMessage(ChatGroupLeftMessage leftMessage) {
		addToConsole("Chat group left message: " + " " + leftMessage.groupId);
	}
	
	public void onChatGroupInvitedMessage(ChatGroupInvitedMessage invitedMessage) {
		addToConsole("Chat group invited message: " + invitedMessage.callerId);
	}
	
	public void onChatGroupUserRemovedMessage(UserRemovedMessage userRemovedMessage) {
		addToConsole("Chat group user removed message: " + userRemovedMessage.removedUserId);
	}
	
	public void onChatGroupUserLeftMessage(UserLeftMessage userLeftMessage) {
		addToConsole("Chat group user left message: " + userLeftMessage.userId);
	}
	
	public void onChatGroupUserJoinedMessage(UserJoinedMessage userJoinedMessage) {
		addToConsole("Chat group user joined message: " + userJoinedMessage.userId);
	}
	
	public void onChatGroupUserAddedMessage(UserAddedMessage userAddedMessage) {
		addToConsole("Chat group user added message: " + userAddedMessage.adderUserId);
	}
	
	public void onChatInvitationMessage(ChatInvitationMessage invitationMessage) {
		addToConsole("Chat invitation message: " + invitationMessage.groupName);
	}
	
	public void onOfflineChatMessage(List<ChatMessage> messageList) {
		addToConsole("Offline Chat Message: " + messageList.Count);
		for (int i = 0; i < messageList.Count; i++) {
			addToConsole("type: " + messageList[i]._type);
			if (messageList[i]._type.Equals(BacktoryConnectivityMessage.SIMPLE_CHAT_MESSAGE)) {
				addToConsole("message: " + ((SimpleChatMessage) messageList[i]).message);
			}
		}
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
				matchNotFoundMessage.requestId + " not found" + " " + matchNotFoundMessage.metaData);
	}
	
	public void onMatchUpdate(MatchUpdateMessage matchUpdateMessage) {
		addToConsole("---Matchmaking with id "
				+ matchUpdateMessage.requestId + " updated");
	}
	
	public void onChatMessage(SimpleChatMessage chatMessage) {
		addToConsole("---Chat message: " + chatMessage.message + " at: " + chatMessage.date);
	}

	public void onConnected(String userId) {
		addToConsole("---Connected: " + "userId: " + userId);
	}
	
	public void onDisconnect() {
		addToConsole("---Disconnect");
	}

	// Update is called once per frame
	void Update () {		
	}
	
	void addToConsole(String str) {
		tConsole.text = str + "\n" + tConsole.text;
	}
}
