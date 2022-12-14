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

    object _userListLock { get; set; }
    object _clientAddLock { get; set; }
    object _loadSceneLock { get; set; }
    object _clientDisconnectLock { get; set; }

    bool triggerClientAdded { get; set; }
    bool triggerClientDisconected { get; set; }
    bool triggerLoadScene { get; set; }

    NetworkUser myNetworkUser { get; set; }
    List<NetworkUser> networkUserList { get; set; }
}