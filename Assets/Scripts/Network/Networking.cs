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

    bool triggerClientAdded { get; set; }
    bool triggerClientDisconected { get; set; }
    bool triggerLoadScene { get; set; }

    NetworkUser myNetworkUser { get; set; }
    List<NetworkUser> networkUserList { get; set; }
}