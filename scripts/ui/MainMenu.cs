using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("StartGame was clicked!");
        SceneManager.LoadScene("LoadToGame");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
