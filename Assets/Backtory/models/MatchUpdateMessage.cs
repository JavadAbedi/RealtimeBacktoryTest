using System;
using System.Collections.Generic;

public class MatchUpdateMessage : BacktoryConnectivityMessage {
    public String requestId;
    public List<MatchFoundParticipant> participants;
}