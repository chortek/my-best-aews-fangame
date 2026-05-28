using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryByKey : MonoBehaviour
{
    public MemoryGameManager gameManager;

    void Update()
    {
        if (gameManager != null && gameManager.GetScore() >= 4 && Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene("VictoryScene");
        }
    }
}