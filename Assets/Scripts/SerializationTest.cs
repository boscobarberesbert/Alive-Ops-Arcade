// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;


// public class Packet
// {
//     public enum PacketType
//     {
//         NONE,
//         CLIENT_NEW,
//         PLAYER_UPDATE
//     }
//     public PacketType type = PacketType.NONE;
// }

// public class ClientData : Packet
// {
//     public int playerID;
//     public Vector3 position;
// }

// public class SerializationTest : MonoBehaviour
// {
//     Packet pck;
//     MemoryStream stream;

//     void SerializeJson(ClientData clientData)
//     {
//         string json = JsonUtility.ToJson(clientData);
//         stream = new MemoryStream();
//         BinaryWriter writer = new BinaryWriter(stream);
//         writer.Write(json);
//     }

//     ClientData DeserializeJson()
//     {
//         BinaryReader reader = new BinaryReader(stream);
//         stream.Seek(0, SeekOrigin.Begin);

//         string json = reader.ReadString();

//         Debug.Log(json);

//         Packet tmp = JsonUtility.FromJson<Packet>(json);

//         Debug.Log(tmp.type);

//         ClientData clientData;
//         if (tmp.type == Packet.PacketType.CLIENT_NEW)
//         {
//             clientData = JsonUtility.FromJson<ClientData>(json);
//             Debug.Log(clientData.type);
//             Debug.Log(clientData.playerID);
//             Debug.Log(clientData.position);
//         }
//         else
//             clientData = null;

//         return clientData;
//     }

//     void Start()
//     {
//         ClientData cD = new ClientData();
//         cD.type = Packet.PacketType.CLIENT_NEW;
//         cD.playerID = 6;
//         cD.position = new Vector3(3f, 3f, 3f);

//         SerializeJson(cD);

//         ClientData cdFinal = new ClientData();
//         cdFinal = DeserializeJson();
//     }
// }