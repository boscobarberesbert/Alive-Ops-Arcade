using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class PlayerData
{
    public static string myIP = "";
    public static string connectToIP = "";
    public static bool client = true;
    public static string username = "";
}

public class MainMenu : MonoBehaviour
{
    public static string serverIp;
    public static string username;
    [SerializeField] Canvas menuCanvas;
    [SerializeField] Canvas joinRoomCanvas;
    [SerializeField] Canvas createRoomCanvas;
    [SerializeField] TMP_Text inputFieldTextConnectToIP;
    [SerializeField] TMP_Text inputFieldTextClientUsername;
    [SerializeField] TMP_Text inputFieldTextServerUsername;

    public void CreateRoomBtn()
    {
        menuCanvas.gameObject.SetActive(false);
        createRoomCanvas.gameObject.SetActive(true);
    }

    public void JoinRoomBtn()
    {
        menuCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
    }

    public void JoinBtn()
    {
        if (inputFieldTextConnectToIP.text.Length > 0 && inputFieldTextClientUsername.text.Length > 0)
        {
            JoinRoom();
        }
    }

    public void CreateBtn()
    {
        if (inputFieldTextConnectToIP.text.Length > 0 && inputFieldTextClientUsername.text.Length > 0)
        {
            CreateRoom();
        }
    }

    private void CreateRoom()
    {
        PlayerData.username = inputFieldTextClientUsername.text.Trim();
        PlayerData.client = false;
        SceneManager.LoadScene("Lobby");
    }

    private void JoinRoom()
    {
        PlayerData.connectToIP = inputFieldTextConnectToIP.text.Trim();
        PlayerData.username = inputFieldTextClientUsername.text.Trim();
        SceneManager.LoadScene("Lobby");
    }
}
