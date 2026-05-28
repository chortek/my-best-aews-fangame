using UnityEngine;

public class ScoreSoundTrigger : MonoBehaviour
{
    [Header("Ссылки")]
    public MemoryGameManager gameManager; // перетащи сюда MemoryGameManager
    public AudioClip targetSound;         // звук для счёта 1 и 4

    [Header("Настройки")]
    public float volume = 0.7f;

    private AudioSource audioSource;
    private bool played1 = false;
    private bool played4 = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (gameManager == null || targetSound == null) return;

        int currentScore = gameManager.GetScore();

        if (currentScore == 1 && !played1)
        {
            PlaySound();
            played1 = true;
        }
        else if (currentScore == 4 && !played4)
        {
            PlaySound();
            played4 = true;
        }
    }

    void PlaySound()
    {
        audioSource.PlayOneShot(targetSound, volume);
        Debug.Log("ScoreSoundTrigger: звук сыгран при счёте " + (gameManager != null ? gameManager.GetScore().ToString() : "?"));
    }
}