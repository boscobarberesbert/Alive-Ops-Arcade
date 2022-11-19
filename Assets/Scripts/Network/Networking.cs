using System.Net;

interface INetworking
{
    // Start is called before the first frame update
    void Start();

    void OnPackageReceived(byte[] inputPacket,int recv,EndPoint fromAddress);


    void OnUpdate();

    void OnConnectionReset(EndPoint fromAddress);

    void SendPacket(byte[] outputPacket, EndPoint toAddress);

    void OnDisconnect();

    void reportError();

}
