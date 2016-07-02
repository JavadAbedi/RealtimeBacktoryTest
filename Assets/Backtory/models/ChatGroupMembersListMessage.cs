using System.Collections.Generic;

public class ChatGroupMembersListMessage : BacktoryConnectivityMessage {
    public List<ChatGroupMember> groupMemberList;
    public string groupId;
}