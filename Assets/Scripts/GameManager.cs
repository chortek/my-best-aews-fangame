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

    [Header("Level Navigation")]
    public string levelUp;
    public string levelDown;

    [Header("Win Screen")]
    public GameObject winPanel;
    public AudioClip winSound;

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

    [Header("Special Notebook (18th)")]
    public bool enableSpecialNotebook = false;
    public string specialNotebookID = "MEMORY";
    public int specialNotebookCount = 18;
    public GameObject specialNotebook;
    public Color specialCounterColor = Color.red;
    private bool specialNotebookRevealed = false;

    private AudioSource calmSource;
    private AudioSource spoopySource;
    private AudioSource chaseStartSource;
    private AudioSource chaseLoopSource;
    private AudioSource phraseSource;
    private AudioSource winSoundSource;

    private bool isSpoopyTriggered = false;
    private bool isChasePhraseDone = false;
    private float spoopyTimer = 0f;
    private bool isGameWon = false;

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

        winSoundSource = gameObject.AddComponent<AudioSource>();
        winSoundSource.spatialBlend = 0f;
        winSoundSource.playOnAwake = false;

        if (reverbFilter == null)
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();

        reverbFilter.enabled = false;
        reverbFilter.reverbPreset = (AudioReverbPreset)1;

        if (winPanel != null)
            winPanel.SetActive(false);
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

            if (enableSpecialNotebook && counter.itemID == specialNotebookID)
            {
                counter.requiredCount = specialNotebookCount - 1;
            }
        }

        isSpoopy = false;
        isChase = false;
        isSpoopyTriggered = false;
        isChasePhraseDone = false;
        spoopyTimer = 0f;
        isGameWon = false;

        specialNotebookRevealed = false;
        if (specialNotebook != null)
            specialNotebook.SetActive(false);

        RenderSettings.fog = false;

        if (reverbFilter != null)
            reverbFilter.enabled = false;

        if (winPanel != null)
            winPanel.SetActive(false);

        Time.timeScale = 1f;
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
                Debug.Log("Reverb enabled on first notebook");
            }
        }

        // isChase при 17
        if (!isChase && !isChasePhraseDone && CheckAllRequiredCollectedExact())
        {
            ActivateChaseSequence();
        }
    }

    void RevealSpecialNotebook()
    {
        if (specialNotebook != null)
        {
            specialNotebook.SetActive(true);
            specialNotebookRevealed = true;

            Counter counter = GetCounter(specialNotebookID);
            if (counter != null)
            {
                counter.requiredCount = specialNotebookCount;
                Debug.Log($"Required count for {specialNotebookID} increased to {specialNotebookCount}");
            }

            UpdateNotebookUI();
            Debug.Log($"18th notebook revealed at {specialNotebook.transform.position}!");
        }
        else
        {
            Debug.LogError("Special notebook is NULL!");
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
        if (Input.GetKeyDown(KeyCode.UpArrow) && !string.IsNullOrEmpty(levelUp))
        {
            SceneManager.LoadScene(levelUp);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && !string.IsNullOrEmpty(levelDown))
        {
            SceneManager.LoadScene(levelDown);
        }

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
            bool isSpecial = enableSpecialNotebook &&
                             counter.itemID == specialNotebookID &&
                             counter.requiredCount == specialNotebookCount &&
                             counter.currentCount == specialNotebookCount - 1;

            if (isSpecial)
            {
                text += $"<color=#{ColorUtility.ToHtmlStringRGB(specialCounterColor)}>{counter.itemID}: {counter.currentCount}/{counter.requiredCount}</color>  ";
            }
            else
            {
                text += $"{counter.itemID}: {counter.currentCount}/{counter.requiredCount}  ";
            }
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

            QualitySettings.SetQualityLevel(0, true);
            Debug.Log($"Fog enabled: {RenderSettings.fog}, Density: {RenderSettings.fogDensity}");
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
        PlayChaseLoopMusic();

        isChase = true;
        OnChaseActivated?.Invoke();

        if (enableSpecialNotebook && !specialNotebookRevealed)
        {
            RevealSpecialNotebook();
            UpdateNotebookUI();
        }

        Debug.Log("isChase activated with: phrase -> chaseStart -> chaseLoop, then 18th notebook revealed");
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

    // ===================== ПОБЕДА =====================

    public void WinGame()
    {
        if (isGameWon) return;
        isGameWon = true;

        // РАЗБЛОКИРОВКА МЫШИ
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winSound != null && winSoundSource != null)
        {
            winSoundSource.PlayOneShot(winSound);
        }

        DisableAllCharacters();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Победа!");
    }

    void DisableAllCharacters()
    {
        TeacherSetup[] teachers = FindObjectsByType<TeacherSetup>(FindObjectsSortMode.None);
        foreach (var teacher in teachers)
        {
            if (teacher != null)
                teacher.gameObject.SetActive(false);
        }
    }

    // ===================== КНОПКИ ПОБЕДНОГО ЭКРАНА =====================

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!string.IsNullOrEmpty(levelUp))
        {
            SceneManager.LoadScene(levelUp);
        }
        else
        {
            Debug.Log("Нет следующего уровня");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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