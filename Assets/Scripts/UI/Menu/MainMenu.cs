using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : Menu
{
    public override string menuName {get; protected set;} = "main";

    public void Play()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("GameScene");
    }

    public void Options()
    {
        MenuManager.instance.OpenMenu("options", menuName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
