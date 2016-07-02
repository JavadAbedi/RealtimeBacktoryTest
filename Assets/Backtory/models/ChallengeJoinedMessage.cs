using System;
using System.Collections.Generic;
public class ChallengeJoinedMessage : BacktoryRealtimeMessage {
    public String userId;
    public String username;
    public List<String> connectedUserIds;
}