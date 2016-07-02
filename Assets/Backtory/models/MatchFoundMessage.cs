using System;
using System.Collections.Generic;

public class MatchFoundMessage : BacktoryConnectivityMessage {
    public String realtimeChallengeId;
    public String address;
    public int port;
    public String matchmakingName;
    public String requestId;
    public List<MatchFoundParticipant> participants;
    public String extraMessage;
    public String ownerUserId;
}