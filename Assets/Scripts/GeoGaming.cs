using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GeoGaming : MonoBehaviour
{
    [Header("Settings")]
    public int rounds = 2;
    public float delayBeforeTask = 1f;

    [Header("UI")]
    public GameObject taskPanel;
    public Text questionText;
    public Button[] answerButtons;
    public Text[] answerTexts;

    [Header("Questions")]
    public List<GeographyQuestion> questions = new List<GeographyQuestion>();

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Audio")]
    public AudioClip activateSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip closeSound;
    public AudioClip abruptSound;

    private List<GeographyQuestion> shuffledQuestions = new List<GeographyQuestion>();
    private int currentRound = 0;
    private int correctAnswers = 0;
    private bool isGameRunning = false;
    private bool isCollected = false;
    private AudioSource audioSource;
    private NotebookSetup notebook;
    private string itemID;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        taskPanel.SetActive(false);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }

        gameObject.SetActive(false);
    }

    public void StartGame(string id, NotebookSetup setup)
    {
        itemID = id;
        notebook = setup;
        isGameRunning = true;
        currentRound = 0;
        correctAnswers = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        shuffledQuestions = new List<GeographyQuestion>(questions);
        Shuffle(shuffledQuestions);

        gameObject.SetActive(true);
        StartCoroutine(DelayBeforeTask());
    }

    IEnumerator DelayBeforeTask()
    {
        yield return new WaitForSeconds(delayBeforeTask);
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentRound >= shuffledQuestions.Count || currentRound >= rounds)
        {
            CompleteGame();
            return;
        }

        GeographyQuestion q = shuffledQuestions[currentRound];

        questionText.text = q.questionText;

        List<string> answers = new List<string> { q.correctAnswer, q.wrongAnswer };
        Shuffle(answers);

        for (int i = 0; i < answerButtons.Length && i < answers.Count; i++)
        {
            answerTexts[i].text = answers[i];
            answerButtons[i].interactable = true;
            answerButtons[i].GetComponent<Image>().color = normalColor;
        }

        Debug.Log($"Question {currentRound + 1}: {q.questionText}");
    }

    void OnAnswerSelected(int index)
    {
        if (!isGameRunning) return;

        string selectedAnswer = answerTexts[index].text;
        GeographyQuestion q = shuffledQuestions[currentRound];
        bool isCorrect = selectedAnswer == q.correctAnswer;

        foreach (var btn in answerButtons)
        {
            btn.interactable = false;
        }

        if (isCorrect)
        {
            answerButtons[index].GetComponent<Image>().color = correctColor;
            if (correctSound != null) audioSource.PlayOneShot(correctSound);
            correctAnswers++;
            Debug.Log($"Correct! ({correctAnswers}/{rounds})");
        }
        else
        {
            answerButtons[index].GetComponent<Image>().color = wrongColor;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerTexts[i].text == q.correctAnswer)
                {
                    answerButtons[i].GetComponent<Image>().color = correctColor;
                    break;
                }
            }

            if (wrongSound != null) audioSource.PlayOneShot(wrongSound);
            Debug.Log("Wrong answer!");
        }

        currentRound++;

        if (currentRound < shuffledQuestions.Count && currentRound < rounds)
        {
            StartCoroutine(DelayBeforeNextQuestion());
        }
        else
        {
            StartCoroutine(DelayBeforeComplete());
        }
    }

    IEnumerator DelayBeforeNextQuestion()
    {
        yield return new WaitForSeconds(1.5f);
        ShowQuestion();
    }

    IEnumerator DelayBeforeComplete()
    {
        yield return new WaitForSeconds(1.5f);
        CompleteGame();
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
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].onClick.RemoveAllListeners();
        }
    }
}

[System.Serializable]
public class GeographyQuestion
{
    public string questionText;
    public string correctAnswer;
    public string wrongAnswer;
}