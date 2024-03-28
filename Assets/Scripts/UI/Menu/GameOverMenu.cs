using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : Menu
{
    public override string menuName {get; protected set;} = "game_over";

    public TextMeshProUGUI scoreUI;

    public void Retry()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("GameScene");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
