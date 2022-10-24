using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public static string serverIp;
    public static string username;
    [SerializeField] Canvas menuCanvas;
    [SerializeField] Canvas joinRoomCanvas;
    [SerializeField] TMP_Text inputFieldTextIP;
    [SerializeField] TMP_Text inputFieldTextName;

    public void CreateRoomBtn()
    {
        CreateRoom();
    }

    public void JoinRoomBtn()
    {
        menuCanvas.gameObject.SetActive(false);
        joinRoomCanvas.gameObject.SetActive(true);
    }

    public void JoinBtn()
    {
        if (inputFieldTextIP.text.Length > 0 && inputFieldTextName.text.Length > 0)
        {
            JoinRoom();
        }
    }

    private void CreateRoom()
    {
        SceneManager.LoadScene("Server");
    }

    private void JoinRoom()
    {
        serverIp = inputFieldTextIP.text.Trim();
        username = inputFieldTextName.text.Trim();
        SceneManager.LoadScene("Client");
    }
}
