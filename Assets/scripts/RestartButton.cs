using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("map1"); // название твоей основной сцены
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}