using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MemoryGaming : MonoBehaviour
{
    [Header("Settings")]
    public int sequenceLength = 4;
    public int rounds = 2;
    public float delayBeforeTask = 1f;
    public float delayBetweenButtons = 0.5f;

    [Header("UI")]
    public Button[] buttons;

    [Header("Colors")]
    public Color[] buttonColors;
    public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip closeSound;
    public AudioClip abruptSound;

    private List<int> currentSequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private int currentStep = 0;
    private int currentRound = 0;
    private bool isPlayerTurn = false;
    private bool isGameRunning = false;
    private Color[] originalColors;
    private AudioSource audioSource;
    private NotebookSetup notebook;
    private string itemID;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        originalColors = new Color[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonPressed(index));

            Image img = buttons[i].GetComponent<Image>();
            if (img != null)
            {
                originalColors[i] = img.color;
            }
            else
            {
                originalColors[i] = (i < buttonColors.Length) ? buttonColors[i] : Color.white;
            }

            SetButtonColor(i, originalColors[i]);
        }

        gameObject.SetActive(false);
    }

    public void StartGame(string id, NotebookSetup setup)
    {
        itemID = id;
        notebook = setup;
        isGameRunning = true;
        currentRound = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.SetActive(true);
        StartCoroutine(RunRound());
    }

    IEnumerator RunRound()
    {
        currentRound++;
        currentStep = 0;
        playerInput.Clear();
        isPlayerTurn = false;

        GenerateSequence();

        // Показываем последовательность
        for (int i = 0; i < currentSequence.Count; i++)
        {
            int index = currentSequence[i];
            SetButtonColor(index, Color.white);
            yield return new WaitForSeconds(delayBetweenButtons);
            SetButtonColor(index, originalColors[index]);
            yield return new WaitForSeconds(0.2f);
        }

        // Ход игрока
        isPlayerTurn = true;
        currentStep = 0;

        while (currentStep < currentSequence.Count)
        {
            yield return null;
        }

        isPlayerTurn = false;

        bool isCorrect = CheckSequence();

        if (isCorrect)
        {
            if (correctSound != null) audioSource.PlayOneShot(correctSound);
        }
        else
        {
            if (wrongSound != null) audioSource.PlayOneShot(wrongSound);
        }

        if (currentRound < rounds)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(RunRound());
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            CompleteGame();
        }
    }

    void GenerateSequence()
    {
        currentSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence.Add(Random.Range(0, buttons.Length));
        }
        Debug.Log($"Sequence: {string.Join(", ", currentSequence)}");
    }

    void OnButtonPressed(int index)
    {
        if (!isPlayerTurn) return;
        if (currentStep >= currentSequence.Count) return;

        SetButtonColor(index, pressedColor);
        StartCoroutine(ResetButtonColor(index));

        playerInput.Add(index);
        currentStep++;

        if (currentStep >= currentSequence.Count)
        {
            isPlayerTurn = false;
            Debug.Log($"Player input: {string.Join(", ", playerInput)}");
        }
    }

    IEnumerator ResetButtonColor(int index)
    {
        yield return new WaitForSeconds(0.2f);
        SetButtonColor(index, originalColors[index]);
    }

    void SetButtonColor(int index, Color color)
    {
        Image img = buttons[index].GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
        else
        {
            Text text = buttons[index].GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = color;
            }
        }
    }

    bool CheckSequence()
    {
        if (playerInput.Count != currentSequence.Count) return false;

        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (playerInput[i] != currentSequence[i])
                return false;
        }
        return true;
    }

    void CompleteGame()
    {
        isGameRunning = false;
        StopAllCoroutines();
        this.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (abruptSound != null)
        {
            audioSource.PlayOneShot(abruptSound);
        }
        else if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        gameObject.SetActive(false);

        if (notebook != null)
        {
            notebook.CompleteGame();
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.RemoveAllListeners();
        }
    }
}