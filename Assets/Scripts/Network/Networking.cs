using System.Collections.Generic;
using System.Net;

public interface INetworking
{
    void Start();

    void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress);

    void OnUpdate();

    void OnConnectionReset(EndPoint fromAddress);

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnDisconnect();

    void reportError();

    object playerMapLock { get; set; }
    object loadSceneLock { get; set; }
    object clientDisconnectLock { get; set; }

    bool triggerClientDisconected { get; set; }
    bool triggerLoadScene { get; set; }

    // Information about the user
    User myUserData { get; set; }
    
    // The player data
    PlayerObject myPlayerData { get; set; }

    // Relates network ID with its player object (the world state basically)
    Dictionary<string, PlayerObject> playerMap { get; set; }
}