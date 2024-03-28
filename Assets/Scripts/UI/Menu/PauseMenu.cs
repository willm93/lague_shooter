using UnityEngine.SceneManagement;

public class PauseMenu : Menu
{
    public override string menuName {get; protected set;} = "pause";

    PlayerInput playerInput;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
    }
   
    public void Resume()
    {
        playerInput.ExternalUnpause();
    }

    public void Restart()
    {
        playerInput.ExternalUnpause();
        SceneManager.LoadScene("GameScene");
    }
    
    public void Options()
    {
        MenuManager.instance.OpenMenu("options", menuName);
    }

    public void Menu()
    {
        playerInput.ExternalUnpause();
        SceneManager.LoadScene("MainMenu");
    }
}
