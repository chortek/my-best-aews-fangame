using UnityEngine;
using TMPro;

public class NotebookCounter : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI counterText;

    [Header("Настройки")]
    public string prefix = "Тетради: ";
    public string suffix = "";
    public int targetScore = 4; // сколько тетрадей нужно собрать

    [Header("Цвета")]
    public Color normalColor = Color.white;
    public Color completedColor = Color.green;

    [Header("Ссылки")]
    public MemoryGameManager gameManager;

    void Update()
    {
        if (gameManager == null || counterText == null) return;

        int current = gameManager.GetScore();
        counterText.text = $"{prefix}{current}/{targetScore}{suffix}";

        // Меняем цвет только счётчика, не затрагивая префикс/суффикс
        if (current >= targetScore)
            counterText.color = completedColor;
        else
            counterText.color = normalColor;
    }
}