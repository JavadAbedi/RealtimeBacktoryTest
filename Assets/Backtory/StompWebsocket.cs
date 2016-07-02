using WebSocketSharp;

public class StompWebsocket{
    public const string CONTENT_TYPE_TEXT = "text/plain";
    public const string CONTENT_TYPE_JSON = "application/json";

    public WebSocket webSocket;
	public string id;

    public StompWebsocket(WebSocket webSocket)
    {
        this.webSocket = webSocket;
    }

    public void connect(string instanceId){
        if (webSocket == null) return;
        string data = "CONNECT\n" +
                        "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                        "\n\0";
        webSocket.Send(data);
    }
    
        public void realtimeConnect(string instanceId, string challengeId){
        if (webSocket == null) return;
        string data = "CONNECT\n" +
                        "X-Backtory-Connectivity-Id:" + instanceId + "\n" +
                        "X-Backtory-Challenge-Id:" + challengeId + "\n" +
                        "\n\0";
        webSocket.Send(data);
    }

    public void send(string message, string contentType = CONTENT_TYPE_TEXT){
        if(webSocket == null)return;
        string data = "SEND\n" +
                      "content-type:" + contentType + "\n" +
                      "\n" +
                      message + "\0";
        webSocket.Send(data);
    }

    public void heartbeat(){
        if (webSocket == null) return;
        string data = "\n";
        webSocket.Send(data);
    }

    public void disconnect(){
        if (webSocket == null) return;
        string data = "DISCONNECT\n" +
                      "\n\0";
        webSocket.Send(data);
    }
}
