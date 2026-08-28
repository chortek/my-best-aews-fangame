using UnityEngine;

public class NotebookSetup : MonoBehaviour
{
    [Header("Settings")]
    public string itemID = "œ‡ÏˇÚ¸";
    public GameObject miniGamePanel;

    [Header("Game Type")]
    public GameType gameType = GameType.Memory;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    [Header("Audio")]
    public AudioClip activateSound;

    private bool isCollected = false;
    private bool isPlayerNear = false;
    private bool isGameActive = false;
    private AudioSource audioSource;

    public enum GameType
    {
        Memory,
        Geography,
        Typing
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

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
        if (isCollected || isGameActive) return;

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
        Debug.Log($"StartGame called on {gameObject.name} at {Time.time} (isGameActive: {isGameActive})");

        if (isCollected) return;
        if (isGameActive) return;

        isGameActive = true;

        if (activateSound != null)
            audioSource.PlayOneShot(activateSound);

        if (miniGamePanel == null)
        {
            Debug.LogError($"miniGamePanel is NULL on {gameObject.name}!");
            return;
        }

        miniGamePanel.SetActive(true);

        // ========== ¬€¡Œ– “»œ¿ »√–€ ==========
        if (gameType == GameType.Memory)
        {
            MemoryGaming memoryGame = miniGamePanel.GetComponent<MemoryGaming>();
            if (memoryGame != null)
            {
                memoryGame.StartGame(itemID, this);
            }
            else
            {
                Debug.LogError("MemoryGaming component not found on miniGamePanel!");
            }
        }
        else if (gameType == GameType.Geography)
        {
            GeoGaming geoGame = miniGamePanel.GetComponent<GeoGaming>();
            if (geoGame != null)
            {
                geoGame.StartGame(itemID, this);
            }
            else
            {
                Debug.LogError("GeoGaming component not found on miniGamePanel!");
            }
        }
        else if (gameType == GameType.Typing)
        {
            PrintGaming printGame = miniGamePanel.GetComponent<PrintGaming>();
            if (printGame != null)
            {
                printGame.StartGame(itemID, this);
            }
            else
            {
                Debug.LogError("PrintGaming component not found on miniGamePanel!");
            }
        }
        // ====================================
    }

    public void CompleteGame()
    {
        if (isCollected) return;
        isCollected = true;
        isGameActive = false;

        // Œ“ Àﬁ◊¿≈Ã ¬—®
        this.enabled = false;                 // <-- Œ“ Àﬁ◊¿≈Ã — –»œ“
        gameObject.SetActive(false);          // <-- ¬€ Àﬁ◊¿≈Ã Œ¡⁄≈ “

        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddToCounter(itemID, 1);
        }

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