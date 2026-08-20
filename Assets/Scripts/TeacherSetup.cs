using UnityEngine;

public class TeacherSetup : MonoBehaviour
{
    [Header("Subject")]
    public string subjectID;
    public bool isMainTeacher = false;

    [Header("Greeting")]
    public AudioClip greetPhrase;

    [Header("Behaviour")]
    public KatScript behaviour;

    private AudioSource audioSource;
    private bool hasGreeted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpoopyActivated += OnSpoopyActivated;
            GameManager.Instance.OnChaseActivated += OnChaseActivated;
        }

        if (greetPhrase != null && !hasGreeted)
        {
            PlayPhrase(greetPhrase);
            hasGreeted = true;
        }

        // Проверка, что behaviour назначен
        if (behaviour == null)
        {
            Debug.LogError($"TeacherSetup: behaviour is NULL on {gameObject.name}");
        }

        // Проверка, что subjectID корректен
        if (GameManager.Instance != null)
        {
            Counter counter = GameManager.Instance.GetCounter(subjectID);
            if (counter == null)
            {
                Debug.LogError($"TeacherSetup: subjectID '{subjectID}' not found in GameManager counters!");
            }
        }
    }

    void Update()
    {
        if (behaviour == null) return;

        // Обновляем количество тетрадей
        Counter counter = GameManager.Instance.GetCounter(subjectID);
        if (counter != null)
        {
            behaviour.UpdateNotebooksCollected(counter.currentCount);
        }

        // Включение/выключение поведения
        bool shouldBeActive = IsSubjectCollected() && GameManager.Instance.isSpoopy;

        if (shouldBeActive && !behaviour.enabled)
        {
            behaviour.enabled = true;
            Debug.Log($"Behaviour enabled for {gameObject.name}");
        }
        else if (!shouldBeActive && behaviour.enabled)
        {
            behaviour.enabled = false;
            Debug.Log($"Behaviour disabled for {gameObject.name}");
        }
    }

    bool IsSubjectCollected()
    {
        if (GameManager.Instance == null) return false;
        Counter counter = GameManager.Instance.GetCounter(subjectID);
        return counter != null && counter.currentCount >= 1;
    }

    void PlayPhrase(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void OnSpoopyActivated() { }
    void OnChaseActivated()
    {
        if (isMainTeacher && behaviour != null)
        {
            behaviour.enabled = true;
        }
    }

    public void OnSubjectCollected(string collectedSubject)
    {
        if (collectedSubject == subjectID)
        {
            Debug.Log($"Teacher {gameObject.name}: subject {subjectID} collected");
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpoopyActivated -= OnSpoopyActivated;
            GameManager.Instance.OnChaseActivated -= OnChaseActivated;
        }
    }
}