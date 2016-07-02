using System.Collections.Generic;

public class GroupChatHistoryMessage : BacktoryConnectivityMessage {
    public List<SimpleChatMessage> messageList;
    public string groupId;
}