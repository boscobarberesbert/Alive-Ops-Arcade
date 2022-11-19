using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class PlayerData
{
    public static string myIP = "";
    public static string connectToIP = "";
    public static bool isClient = true;
    public static string username = "";
}

public class MainMenu : MonoBehaviour
{
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
        if (inputFieldTextServerUsername.text.Length > 0)
        {
            CreateRoom();
        }
    }

    private void CreateRoom()
    {
        string name = inputFieldTextServerUsername.text.Trim();
        PlayerData.username = name.Substring(0, name.Length - 1);
        PlayerData.isClient = false;

        SceneManager.LoadScene("Lobby");
    }

    private void JoinRoom()
    {
        string inputText = inputFieldTextConnectToIP.text.Trim();
        PlayerData.connectToIP = inputText.Substring(0, inputText.Length - 1);

        inputText = inputFieldTextClientUsername.text.Trim();
        PlayerData.username = inputText.Substring(0, inputText.Length - 1);

        SceneManager.LoadScene("Lobby");
    }
}