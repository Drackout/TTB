using UnityEngine;
using UnityEngine.SceneManagement;

public class Navi : MonoBehaviour
{    
    public void GoSingle()
    {
        Debug.Log("Single Player");
        SceneManager.LoadScene("TTB");
    }
    
    public void GoClient()
    {
        Debug.Log("Multiplayer (Client)");
        SceneManager.LoadScene("ClientWindow");
    }

    public void GoHost()
    {
        Debug.Log("Multiplayer (Host)");
        SceneManager.LoadScene("HostWindow");
    }

    public void GoMainMenu()
    {
        Debug.Log("Go Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
    
    public void GoExit()
    {
        Debug.Log("Exiting the game");
        Application.Quit();
    }
}
