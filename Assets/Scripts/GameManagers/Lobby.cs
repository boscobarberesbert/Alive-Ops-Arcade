using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [SerializeField] GameObject startGameButton;
    // Start is called before the first frame update
    void Start()
    {
        if (MainMenuInfo.isClient)
        {
             startGameButton.SetActive(false);
        }
    }

    public void OnStartGamePressed()
    {
        NetworkingManager.Instance.networking.LoadScene("Game");
    }
}
