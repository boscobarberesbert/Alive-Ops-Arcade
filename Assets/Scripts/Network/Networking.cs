using System.Net;

public interface INetworking
{
    // Start is called before the first frame update
    void Start();

    void OnPackageReceived(byte[] inputPacket,int recv,EndPoint fromAddress);

    void OnUpdate();

    void OnConnectionReset(EndPoint fromAddress);

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnDisconnect();

    void reportError();

    bool triggerClientAdded { get; set; }

    UserData myUserData { get; set; }

    LobbyState lobbyState { get; set; }
}
