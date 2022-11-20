using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class MainMenuInfo
{
    public static string username = "";
    public static bool isClient = true;
    public static string connectToIp = "";
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
        MainMenuInfo.username = name.Substring(0, name.Length - 1);
        MainMenuInfo.isClient = false;

        SceneManager.LoadScene("Lobby");
    }

    private void JoinRoom()
    {
        string inputText = inputFieldTextConnectToIP.text.Trim();
        MainMenuInfo.connectToIp = inputText.Substring(0, inputText.Length - 1);

        inputText = inputFieldTextClientUsername.text.Trim();
        MainMenuInfo.username = inputText.Substring(0, inputText.Length - 1);

        SceneManager.LoadScene("Lobby");
    }
}