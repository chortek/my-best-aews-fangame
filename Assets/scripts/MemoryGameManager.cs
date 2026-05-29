using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MemoryGameManager : MonoBehaviour
{
    public Button[] buttons;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI scoreText;
    public GameObject memoryGameUI;
    public GameObject player;

    private List<int> currentSequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool waitingForInput = false;
    private int correctSequences = 0;
    private int score = 0;
    private GameObject activeNotebook;

    void Start()
    {
        if (memoryGameUI != null)
            memoryGameUI.SetActive(false);
        UpdateScoreUI();
    }

    public void StartGame(GameObject notebook)
    {
        activeNotebook = notebook;
        LockPlayer(true);
        correctSequences = 0;
        NextRound();
    }

    void NextRound()
    {
        if (correctSequences >= 2)
        {
            score++;
            UpdateScoreUI();
            EndGame();
            return;
        }

        GenerateSequence();
        StartCoroutine(ShowSequence());
    }

    void GenerateSequence()
    {
        currentSequence.Clear();
        for (int i = 0; i < 3; i++)
            currentSequence.Add(Random.Range(0, buttons.Length));
    }

    IEnumerator ShowSequence()
    {
        waitingForInput = false;
        if (statusText != null) statusText.text = "Look...";

        foreach (int i in currentSequence)
        {
            if (buttons[i] == null) continue;
            Color c = buttons[i].image.color;
            buttons[i].image.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            buttons[i].image.color = c;
            yield return new WaitForSeconds(0.2f);
        }

        if (statusText != null) statusText.text = "Play!";
        playerInput.Clear();
        waitingForInput = true;
    }

    public void OnButtonClick(int index)
    {
        if (!waitingForInput) return;

        playerInput.Add(index);

        if (playerInput[playerInput.Count - 1] != currentSequence[playerInput.Count - 1])
        {
            if (statusText != null) statusText.text = "Again! Look...";
            waitingForInput = false;
            StartCoroutine(ShowSequence());
            return;
        }

        if (playerInput.Count == currentSequence.Count)
        {
            waitingForInput = false;
            correctSequences++;
            if (statusText != null) statusText.text = "Correct! Look...";
            NextRound();
        }
    }

    void EndGame()
    {
        if (statusText != null) statusText.text = "Correct!";
        StartCoroutine(FadeOutAndClose());

        if (activeNotebook != null)
            activeNotebook.SetActive(false);
    }

    IEnumerator FadeOutAndClose()
    {
        CanvasGroup canvasGroup = memoryGameUI.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        if (memoryGameUI != null)
            memoryGameUI.SetActive(false);
        LockPlayer(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            if (score >= 4)
                scoreText.text = $"Memory: 4/Press P to win!";
            else
                scoreText.text = "Memory: " + score + "/4";
        }
    }

    void LockPlayer(bool lockIt)
    {
        if (player == null) return;
        MonoBehaviour move = player.GetComponent("PlayerMovement") as MonoBehaviour;
        MonoBehaviour look = player.GetComponent("MouseLook") as MonoBehaviour;
        if (move != null) move.enabled = !lockIt;
        if (look != null) look.enabled = !lockIt;

        if (lockIt)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public int GetScore()
    {
        return score;
    }
    public void AddScore(int value)
    {
        score += value;
        if (score < 0) score = 0;
        UpdateScoreUI();
    }

}