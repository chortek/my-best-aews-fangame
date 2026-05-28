using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    public AudioClip stepSound;        // перетащи звук сюда (один раз)
    public float stepInterval = 0.5f;  // шаг каждые 0.5 секунды
    public float volume = 0.5f;

    private CharacterController controller;
    private AudioSource audioSource;
    private float timer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (controller == null || stepSound == null) return;

        // Движение?
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && controller.isGrounded;

        if (!isMoving)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            audioSource.PlayOneShot(stepSound, volume);
        }
    }
}