using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("LD_POC");

    }
    
    public void QuitGame()
    {
        Application.Quit();

    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }

}
