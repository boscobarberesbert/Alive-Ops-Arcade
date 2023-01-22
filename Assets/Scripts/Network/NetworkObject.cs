using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public string networkID = "";
    private void Awake()
    {
        networkID = System.Guid.NewGuid().ToString();
    }
}