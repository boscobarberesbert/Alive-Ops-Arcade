using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessage
{
    public string sender;
    public string message;
    public ChatMessage(string sender, string message)
    {
        this.sender = sender;
        this.message = message;
    }
    
}
