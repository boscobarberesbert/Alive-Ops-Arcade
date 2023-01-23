using System.Collections.Generic;
using System.Net;

public interface INetworking
{
    void Start();

    void OnPackageReceived(byte[] inputPacket, int recv, EndPoint fromAddress);

    void OnUpdate();

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnConnectionReset(EndPoint fromAddress);

    void OnDisconnect();

    void reportError();

    void LoadScene(string sceneName);

    void OnSceneLoaded();

    void OnShoot();

    object packetQueueLock { get; set; }

    object loadSceneLock { get; set; }
    bool triggerLoadScene { get; set; }

    object clientDisconnectLock { get; set; }
    bool triggerClientDisconected { get; set; }

    // Information about the user: networkID, username
    User myUserData { get; set; }

    // Queue of received packets
    Queue<Packet> packetQueue { get; set; }
}