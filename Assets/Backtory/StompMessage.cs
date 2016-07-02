using UnityEngine;
using System.Collections;

public class StompMessage{
    public string text;
    public string contentType;

    public StompMessage(string text, string contentType = StompWebsocket.CONTENT_TYPE_TEXT){
        this.text = text;
        this.contentType = contentType;
    }

}
