using UnityEngine;

public class NotebookSetup : MonoBehaviour
{
    [Header("Settings")]
    public string itemID = "œ‡ÏˇÚ¸";
    public GameObject miniGamePanel;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    [Header("Audio")]
    public AudioClip activateSound;

    private bool isCollected = false;
    private bool isPlayerNear = false;
    private bool isGameActive = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Õ≈ Œ“ Àﬁ◊¿≈Ã œ¿Õ≈À‹ ¬ START()
        // if (miniGamePanel != null)
        //     miniGamePanel.SetActive(false);

        if (miniGamePanel != null)
        {
            Debug.Log($"Panel: {miniGamePanel.name}, active: {miniGamePanel.activeSelf}");
        }
        else
        {
            Debug.LogError("miniGamePanel is NULL on " + gameObject.name);
        }
    }

    void Update()
    {
        if (isCollected) return;

        // “≈—“: Ì‡ÊÏË T ‰Îˇ Û˜ÌÓ„Ó Á‡ÔÛÒÍ‡
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Manual start by T");
            StartGame();
        }

        CheckPlayerInTrigger();

        if (isPlayerNear && !isGameActive && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed, starting game...");
            StartGame();
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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger: {other.name}, tag: {other.tag}");
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;
        Debug.Log("Player entered trigger zone");
    }

    void StartGame()
    {
        Debug.Log($"StartGame on {gameObject.name}");

        isGameActive = true;

        if (activateSound != null)
            audioSource.PlayOneShot(activateSound);

        if (miniGamePanel == null)
        {
            Debug.LogError($"miniGamePanel is NULL on {gameObject.name}!");
            return;
        }

        Debug.Log($"Panel: {miniGamePanel.name}, active before: {miniGamePanel.activeSelf}");
        miniGamePanel.SetActive(true);
        Debug.Log($"Panel active after: {miniGamePanel.activeSelf}");

        MemoryGaming game = miniGamePanel.GetComponent<MemoryGaming>();
        if (game != null)
        {
            game.StartGame(itemID, this);
        }
        else
        {
            Debug.LogError("MemoryGaming component not found on miniGamePanel!");
        }
    }

    public void CompleteGame()
    {
        isCollected = true;
        isGameActive = false;

        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddToCounter(itemID, 1);
        }

        gameObject.SetActive(false);
        Debug.Log($"Notebook {gameObject.name}: game completed");
    }

    public void CancelGame()
    {
        isGameActive = false;
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);
        Debug.Log($"Notebook {gameObject.name}: game cancelled");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}