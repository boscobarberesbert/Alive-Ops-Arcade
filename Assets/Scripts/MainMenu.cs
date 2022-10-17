using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public static string serverIp;
    [SerializeField] Canvas menuCanvas;
    [SerializeField] Canvas joinRoomCanvas;
    [SerializeField] TMP_Text inputFieldText;
    
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
        if (inputFieldText.text.Length > 0)
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
        serverIp = inputFieldText.text.Trim();
        SceneManager.LoadScene("Client");
    }
}
