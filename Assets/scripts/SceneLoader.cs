using UnityEngine;
using TMPro;
using UnityEngine.UI;        // ← для Button
using UnityEngine.SceneManagement;

public class SceneLoader_TMP : MonoBehaviour
{
    [Header("UI (TMP)")]
    public TMP_InputField inputField;
    public Button loadButton;
    public TMP_Text messageText;

    void Start()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadSceneByName);

        if (inputField != null)
            inputField.onEndEdit.AddListener((value) => LoadSceneByName());
    }

    void LoadSceneByName()
    {
        if (inputField == null) return;

        string sceneName = inputField.text.Trim();

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Название сцены не введено!");
            if (messageText != null) messageText.text = "Введите название сцены!";
            return;
        }

        if (IsSceneExists(sceneName))
        {
            Debug.Log("Загружаем сцену: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Сцены с именем '" + sceneName + "' не существует в Build Settings!");
            if (messageText != null) messageText.text = "Сцены \"" + sceneName + "\" не существует!";
        }
    }

    bool IsSceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }
}