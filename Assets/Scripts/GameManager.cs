using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isSpoopy { get; private set; } = false;
    public bool isChase { get; private set; } = false;

    public System.Action OnSpoopyActivated;
    public System.Action OnChaseActivated;

    [Header("Счётчики")]
    public List<Counter> counters = new List<Counter>();

    [Header("HUD")]
    public Text notebookCounterText;

    [Header("Музыка (перетащи аудиоклипы)")]
    public AudioClip calmMusic;        // спокойная
    public AudioClip spoopyMusic;      // жуткая
    public AudioClip chaseStartMusic;  // старт погони (набирает обороты)
    public AudioClip chaseLoopMusic;   // зацикленная погоня

    [Header("Фразы (перетащи аудиоклипы)")]
    public AudioClip spoopyPhrase;
    public AudioClip chasePhrase;

    // AudioSource для музыки (создаются автоматически)
    private AudioSource calmSource;
    private AudioSource spoopySource;
    private AudioSource chaseStartSource;
    private AudioSource chaseLoopSource;

    // AudioSource для фраз (2D)
    private AudioSource phraseSource;

    private bool isSpoopyPhraseDone = false;
    private bool isChasePhraseDone = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Создаём AudioSource для музыки
        calmSource = CreateAudioSource("CalmMusic");
        spoopySource = CreateAudioSource("SpoopyMusic");
        chaseStartSource = CreateAudioSource("ChaseStartMusic");
        chaseLoopSource = CreateAudioSource("ChaseLoopMusic");

        // AudioSource для фраз
        phraseSource = gameObject.AddComponent<AudioSource>();
        phraseSource.spatialBlend = 0f;
        phraseSource.playOnAwake = false;
    }

    AudioSource CreateAudioSource(string name)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        return source;
    }

    void Start()
    {
        foreach (var counter in counters)
        {
            counter.currentCount = 0;
            counter.isComplete = false;
        }
        UpdateNotebookUI();

        // Включаем спокойную музыку
        PlayCalmMusic();
    }

    // --- МУЗЫКА ---
    void PlayCalmMusic()
    {
        if (calmMusic != null)
        {
            calmSource.clip = calmMusic;
            calmSource.loop = true;
            calmSource.Play();
        }
    }

    void PlaySpoopyMusic()
    {
        if (spoopyMusic != null)
        {
            calmSource.Stop();
            spoopySource.clip = spoopyMusic;
            spoopySource.loop = true;
            spoopySource.Play();
        }
    }

    void PlayChaseStartMusic()
    {
        if (chaseStartMusic != null)
        {
            spoopySource.Stop();
            chaseStartSource.clip = chaseStartMusic;
            chaseStartSource.loop = false;
            chaseStartSource.Play();
        }
    }

    void PlayChaseLoopMusic()
    {
        if (chaseLoopMusic != null)
        {
            chaseLoopSource.clip = chaseLoopMusic;
            chaseLoopSource.loop = true;
            chaseLoopSource.Play();
        }
    }

    // --- СЧЁТЧИКИ ---
    public void AddToCounter(string itemID, int amount = 1)
    {
        Counter counter = GetCounter(itemID);
        if (counter == null) return;

        counter.currentCount += amount;
        if (counter.requiredCount > 0 && counter.currentCount >= counter.requiredCount)
        {
            counter.isComplete = true;
        }

        UpdateNotebookUI();

        if (!isSpoopy && !isSpoopyPhraseDone && counter.currentCount >= 1)
        {
            ActivateSpoopySequence();
        }

        if (!isChase && !isChasePhraseDone && CheckAllRequiredCollected())
        {
            ActivateChaseSequence();
        }
    }

    public Counter GetCounter(string itemID)
    {
        return counters.Find(c => c.itemID == itemID);
    }

    bool CheckAllRequiredCollected()
    {
        foreach (var counter in counters)
        {
            if (counter.isRequired && !counter.isComplete)
                return false;
        }
        return true;
    }

    // --- UI ---
    void UpdateNotebookUI()
    {
        if (notebookCounterText == null) return;

        string text = "";
        foreach (var counter in counters)
        {
            if (counter.requiredCount > 0)
            {
                text += $"{counter.itemID}: {counter.currentCount}/{counter.requiredCount}  ";
            }
        }
        notebookCounterText.text = text;
    }

    // --- ПОСЛЕДОВАТЕЛЬНОСТЬ isSpoopy ---
    void ActivateSpoopySequence()
    {
        if (isSpoopy || isSpoopyPhraseDone) return;
        isSpoopyPhraseDone = true;

        if (spoopyPhrase != null)
        {
            phraseSource.PlayOneShot(spoopyPhrase);
            float delay = spoopyPhrase.length;
            Invoke(nameof(FinishSpoopy), delay);
        }
        else
        {
            FinishSpoopy();
        }
    }

    void FinishSpoopy()
    {
        PlaySpoopyMusic();
        isSpoopy = true;
        OnSpoopyActivated?.Invoke();
    }

    // --- ПОСЛЕДОВАТЕЛЬНОСТЬ isChase ---
    void ActivateChaseSequence()
    {
        if (isChase || isChasePhraseDone) return;
        isChasePhraseDone = true;

        if (chasePhrase != null)
        {
            phraseSource.PlayOneShot(chasePhrase);
            float delay = chasePhrase.length;
            Invoke(nameof(StartChaseMusic), delay);
        }
        else
        {
            StartChaseMusic();
        }
    }

    void StartChaseMusic()
    {
        PlayChaseStartMusic();

        float startLength = chaseStartMusic != null ? chaseStartMusic.length : 2f;
        Invoke(nameof(FinishChase), startLength);
    }

    void FinishChase()
    {
        PlayChaseLoopMusic();
        isChase = true;
        OnChaseActivated?.Invoke();
    }

    // --- ФРАЗЫ (ручной запуск) ---
    public void PlayPhrase(AudioClip clip)
    {
        if (clip == null) return;
        phraseSource.PlayOneShot(clip);
    }
}

[System.Serializable]
public class Counter
{
    public string itemID;
    public int currentCount = 0;
    public int requiredCount = 0;
    public bool isRequired = false;
    public bool isComplete = false;
}