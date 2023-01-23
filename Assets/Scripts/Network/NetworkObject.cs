using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public string networkID = "";

    void Awake()
    {
        networkID = System.Guid.NewGuid().ToString();
    }
}