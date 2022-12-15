using System.Collections.Generic;
using System.Net;

public interface INetworking
{
    // Start is called before the first frame update
    void Start();

    void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress);

    void OnUpdate();

    void OnConnectionReset(EndPoint fromAddress);

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnDisconnect();

    void reportError();

    object userListLock { get; set; }
    object clientAddLock { get; set; }
    object loadSceneLock { get; set; }
    object clientDisconnectLock { get; set; }

    bool triggerClientAdded { get; set; }
    bool triggerClientDisconected { get; set; }
    bool triggerLoadScene { get; set; }

    // Information about the user
    UserData myUserData { get; set; }
    
    // The user's player
    DynamicObject myPlayer { get; set; }

    // Relates client ID with its player object networkID
    public Dictionary<int, string> playerMap { get; set; }
}