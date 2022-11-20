using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [SerializeField] Button startGameButton;
    // Start is called before the first frame update
    void Start()
    {
        if (MainMenuInfo.isClient)
        {
            startGameButton.enabled = false;
        }
    }

    public void OnStartGamePressed()
    {
        NetworkingManager.Instance.LoadScene("Game");
    }
}
