using UnityEngine;
using UnityEngine.SceneManagement;

public class WinButton : MonoBehaviour
{
    [Header("Settings")]
    public string nextLevel = "";
    public AudioClip winSound;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    private AudioSource audioSource;
    private bool isPlayerNear = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        CheckPlayerInTrigger();

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Win();
        }
    }

    void CheckPlayerInTrigger()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        isPlayerNear = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                isPlayerNear = true;
                break;
            }
        }
    }

    void Win()
    {
        Debug.Log("Player won the level!");

        if (winSound != null)
            audioSource.PlayOneShot(winSound);

        if (!string.IsNullOrEmpty(nextLevel))
        {
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}