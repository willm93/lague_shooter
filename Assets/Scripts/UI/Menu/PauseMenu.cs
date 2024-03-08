using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : Menu
{
    public override string menuName {get; protected set;} = "pause";

    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }
   
    public void Resume()
    {
        player.ExternalUnpause();
    }

    public void Restart()
    {
        player.ExternalUnpause();
        SceneManager.LoadScene("GameScene");
    }
    
    public void Options()
    {
        MenuManager.instance.OpenMenu("options", menuName);
    }

    public void Menu()
    {
        player.ExternalUnpause();
        SceneManager.LoadScene("MainMenu");
    }
}
