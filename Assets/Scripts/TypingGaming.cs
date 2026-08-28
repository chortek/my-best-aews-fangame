using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PrintGaming : MonoBehaviour
{
    [Header("Settings")]
    public List<PrintTask> tasks = new List<PrintTask>();
    public int rounds = 2;                      // Количество слов
    public float delayBeforeTask = 1f;
    public float delayAfterAnswer = 1f;

    [Header("UI")]
    public GameObject taskPanel;
    public Text targetText;
    public InputField inputField;
    public Button submitButton;
    public Text feedbackText;

    [Header("Colors")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Audio")]
    public AudioClip activateSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip closeSound;
    public AudioClip abruptSound;

    private List<PrintTask> shuffledTasks = new List<PrintTask>();
    private int currentTaskIndex = 0;
    private bool isGameRunning = false;
    private AudioSource audioSource;
    private NotebookSetup notebook;
    private string itemID;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        taskPanel.SetActive(false);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmit);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameRunning && Input.GetKeyDown(KeyCode.Return))
        {
            OnSubmit();
        }
    }

    public void StartGame(string id, NotebookSetup setup)
    {
        itemID = id;
        notebook = setup;
        isGameRunning = true;
        currentTaskIndex = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        shuffledTasks = new List<PrintTask>(tasks);
        Shuffle(shuffledTasks);

        // Ограничиваем количество раундов
        if (shuffledTasks.Count > rounds)
        {
            shuffledTasks = shuffledTasks.GetRange(0, rounds);
        }

        Debug.Log($"Print started: {shuffledTasks.Count} tasks");

        gameObject.SetActive(true);
        StartCoroutine(DelayBeforeTask());
    }

    IEnumerator DelayBeforeTask()
    {
        yield return new WaitForSeconds(delayBeforeTask);
        ShowTask();
    }

    void ShowTask()
    {
        if (currentTaskIndex >= shuffledTasks.Count)
        {
            CompleteGame();
            return;
        }

        PrintTask task = shuffledTasks[currentTaskIndex];
        targetText.text = task.targetText;
        inputField.text = "";
        feedbackText.text = "";
        inputField.interactable = true;
        submitButton.interactable = true;
        inputField.Select();
        inputField.ActivateInputField();

        Debug.Log($"Task {currentTaskIndex + 1}/{shuffledTasks.Count}: print '{task.targetText}'");
    }

    void OnSubmit()
    {
        if (!isGameRunning) return;

        string input = inputField.text.Trim();
        PrintTask task = shuffledTasks[currentTaskIndex];

        if (string.IsNullOrEmpty(input))
        {
            feedbackText.text = "TYPE THIS!";
            feedbackText.color = wrongColor;
            return;
        }

        if (input.Equals(task.targetText, System.StringComparison.OrdinalIgnoreCase))
        {
            feedbackText.text = "CORRECT!";
            feedbackText.color = correctColor;
            if (correctSound != null) audioSource.PlayOneShot(correctSound);

            inputField.interactable = false;
            submitButton.interactable = false;

            currentTaskIndex++;

            if (currentTaskIndex < shuffledTasks.Count)
            {
                StartCoroutine(DelayBeforeNextTask());
            }
            else
            {
                StartCoroutine(DelayBeforeComplete());
            }
        }
        else
        {
            feedbackText.text = "NOPE! AGAIN";
            feedbackText.color = wrongColor;
            if (wrongSound != null) audioSource.PlayOneShot(wrongSound);

            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    IEnumerator DelayBeforeNextTask()
    {
        yield return new WaitForSeconds(delayAfterAnswer);
        ShowTask();
    }

    IEnumerator DelayBeforeComplete()
    {
        yield return new WaitForSeconds(delayAfterAnswer);
        CompleteGame();
    }

    void CompleteGame()
    {
        isGameRunning = false;
        StopAllCoroutines();                     // <-- ОСТАНАВЛИВАЕМ ВСЕ КОРУТИНЫ
        this.enabled = false;                   // <-- ОТКЛЮЧАЕМ СКРИПТ

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

        gameObject.SetActive(false);            // <-- ВЫКЛЮЧАЕМ ОБЪЕКТ

        if (notebook != null)
        {
            notebook.CompleteGame();
        }

        Debug.Log($"Typing completed! {shuffledTasks.Count} tasks done. Yuy!");
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void OnDestroy()
    {
        if (submitButton != null)
            submitButton.onClick.RemoveListener(OnSubmit);
    }
}

[System.Serializable]
public class PrintTask
{
    public string targetText;
}