using UnityEngine;

public class ShuamaSoundOnce : MonoBehaviour
{
    [Header("Звук")]
    public AudioClip shuamaSound;
    public float volume = 1f;

    [Header("Задержка перед звуком (сек)")]
    public float delay = 0f;

    private static bool played = false; // флаг, чтобы звук проигрывался только один раз

    void Start()
    {
        if (!played && shuamaSound != null)
        {
            played = true;

            if (delay > 0f)
                Invoke(nameof(PlaySound), delay);
            else
                PlaySound();
        }
    }

    void PlaySound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.PlayOneShot(shuamaSound, volume);
        Debug.Log("Shuama звук проигран один раз при запуске");
    }
}