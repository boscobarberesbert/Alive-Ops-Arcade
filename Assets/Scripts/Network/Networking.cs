using System.Collections.Generic;
using System.Net;

public interface INetworking
{
    void Start();

    void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress);

    void UpdatePlayerState();

    void OnUpdate();

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnConnectionReset(EndPoint fromAddress);

    void OnDisconnect();

    void reportError();

    object playerMapLock { get; set; }

    object loadSceneLock { get; set; }
    bool triggerLoadScene { get; set; }

    object clientDisconnectLock { get; set; }
    bool triggerClientDisconected { get; set; }

    // Information about the user: networkID, username
    User myUserData { get; set; }

    // Relates network ID with its player object (the world state basically)
    Dictionary<string, PlayerObject> playerMap { get; set; }
}