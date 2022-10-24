using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessage
{
    public string sender;
    public string message;
    public string username;
    public ChatMessage(string sender, string message, string username)
    {
        this.sender = sender;
        this.message = message;
        this.username = username;
    }
}
