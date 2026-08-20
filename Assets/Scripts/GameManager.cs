using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isSpoopy { get; private set; } = false;
    public bool isChase { get; private set; } = false;

    public System.Action OnSpoopyActivated;
    public System.Action OnChaseActivated;

    [Header("Counters")]
    public List<Counter> counters = new List<Counter>();

    [Header("HUD")]
    public Text notebookCounterText;

    [Header("Music")]
    public AudioClip calmMusic;
    public AudioClip spoopyMusic;
    public AudioClip chaseStartMusic;
    public AudioClip chaseLoopMusic;

    [Header("Phrases")]
    public AudioClip spoopyPhrase;
    public AudioClip chasePhrase;

    [Header("Spoopy Settings")]
    public float spoopyDelay = 15f;

    [Header("Fog")]
    public bool enableFogOnSpoopy = true;
    public Color fogColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public float fogDensity = 0.05f;
    public FogMode fogMode = FogMode.Exponential;

    [Header("Reverb")]
    public AudioReverbFilter reverbFilter;
    public bool enableReverbOnSpoopy = true;
    [Range(0, 7)] public int reverbPreset = 1;

    [Header("Test Buttons")]
    public bool enableTestButtons = false;
    public KeyCode[] testKeys = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5
    };

    private AudioSource calmSource;
    private AudioSource spoopySource;
    private AudioSource chaseStartSource;
    private AudioSource chaseLoopSource;
    private AudioSource phraseSource;

    private bool isSpoopyTriggered = false;
    private bool isChasePhraseDone = false;
    private float spoopyTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        calmSource = CreateAudioSource("CalmMusic");
        spoopySource = CreateAudioSource("SpoopyMusic");
        chaseStartSource = CreateAudioSource("ChaseStartMusic");
        chaseLoopSource = CreateAudioSource("ChaseLoopMusic");

        phraseSource = gameObject.AddComponent<AudioSource>();
        phraseSource.spatialBlend = 0f;
        phraseSource.playOnAwake = false;

        if (reverbFilter == null)
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();

        reverbFilter.enabled = false;
        reverbFilter.reverbPreset = (AudioReverbPreset)reverbPreset;
    }

    void Start()
    {
        ResetState();
        UpdateNotebookUI();
        PlayCalmMusic();
    }

    public void ResetState()
    {
        foreach (var counter in counters)
        {
            counter.currentCount = 0;
            counter.isComplete = false;
        }

        isSpoopy = false;
        isChase = false;
        isSpoopyTriggered = false;
        isChasePhraseDone = false;
        spoopyTimer = 0f;

        RenderSettings.fog = false;

        if (reverbFilter != null)
            reverbFilter.enabled = false;
    }

    AudioSource CreateAudioSource(string name)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        return source;
    }

    void PlayCalmMusic()
    {
        if (calmMusic != null)
        {
            calmSource.clip = calmMusic;
            calmSource.loop = true;
            calmSource.Play();
        }
    }

    void StopCalmMusicInstant()
    {
        calmSource.Stop();
        calmSource.clip = null;
    }

    void PlaySpoopyMusic()
    {
        if (spoopyMusic != null)
        {
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

        if (!isSpoopy && !isSpoopyTriggered && counter.currentCount >= 1)
        {
            isSpoopyTriggered = true;
            spoopyTimer = 0f;
            StopCalmMusicInstant();

            if (enableReverbOnSpoopy && reverbFilter != null)
            {
                reverbFilter.enabled = true;
                reverbFilter.reverbPreset = (AudioReverbPreset)reverbPreset;
            }
        }

        if (!isChase && !isChasePhraseDone && CheckAllRequiredCollectedExact())
        {
            ActivateChaseSequence();
        }
    }

    bool CheckAllRequiredCollectedExact()
    {
        foreach (var counter in counters)
        {
            if (counter.isRequired && counter.currentCount != counter.requiredCount)
                return false;
        }
        return true;
    }

    public Counter GetCounter(string itemID)
    {
        return counters.Find(c => c.itemID == itemID);
    }

    void Update()
    {
        if (isSpoopyTriggered && !isSpoopy)
        {
            spoopyTimer += Time.deltaTime;
            if (spoopyTimer >= spoopyDelay)
            {
                ActivateSpoopySequence();
            }
        }

        if (enableTestButtons)
        {
            for (int i = 0; i < counters.Count && i < testKeys.Length; i++)
            {
                if (Input.GetKeyDown(testKeys[i]))
                {
                    AddToCounter(counters[i].itemID, 1);
                }
            }
        }
    }

    void UpdateNotebookUI()
    {
        if (notebookCounterText == null) return;

        string text = "";
        foreach (var counter in counters)
        {
            // Показываем ВСЕ счётчики, даже если requiredCount = 0
            text += $"{counter.itemID}: {counter.currentCount}/{counter.requiredCount}  ";
        }
        notebookCounterText.text = text.Trim();
    }

    void ActivateSpoopySequence()
    {
        if (isSpoopy) return;
        isSpoopyTriggered = false;

        if (spoopyPhrase != null)
        {
            phraseSource.PlayOneShot(spoopyPhrase);
        }

        PlaySpoopyMusic();

        if (enableFogOnSpoopy)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
        }

        isSpoopy = true;
        OnSpoopyActivated?.Invoke();
    }

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
        if (chaseStartMusic != null)
        {
            PlayChaseStartMusic();
            float startLength = chaseStartMusic.length;
            Invoke(nameof(FinishChase), startLength);
        }
        else
        {
            FinishChase();
        }
    }

    void FinishChase()
    {
        if (chaseLoopMusic != null)
        {
            PlayChaseLoopMusic();
        }

        isChase = true;
        OnChaseActivated?.Invoke();
    }

    public void PlayPhrase(AudioClip clip)
    {
        if (clip == null) return;
        phraseSource.PlayOneShot(clip);
    }

    public void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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