using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessage
{
    public string senderType;
    public string message;
    public string senderName;
    public ChatMessage(string senderType, string message, string senderName)
    {
        this.senderType = senderType;
        this.message = message;
        this.senderName = senderName;
    }
}
