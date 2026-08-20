using UnityEngine;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 openOffset = new Vector3(2f, 0f, 0f);
    public float speed = 3f;
    public float openDuration = 5f;

    [Header("Requirements")]
    public List<DoorRequirement> requirements;

    [Header("Audio")]
    public AudioClip openSound;
    [Range(0f, 1f)] public float openSoundVolume = 1f;
    public float soundMaxDistance = 20f;

    [Header("Settings")]
    public bool openOnStart = false;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private float closeTimer = 0f;
    private bool playerNear = false;
    private bool teacherNear = false;
    private AudioSource audioSource;
    private bool hasTriedOpen = false;

    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = soundMaxDistance;
        audioSource.volume = openSoundVolume;
        audioSource.playOnAwake = false;

        // Œƒ»Õ –¿« œ–» —“¿–“≈
        if (openOnStart && !hasTriedOpen)
        {
            hasTriedOpen = true;
            TryOpen();
            Debug.Log($"Door {gameObject.name}: TryOpen called on start");
        }
    }

    void Update()
    {
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (isOpen)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                CloseDoor();
            }
        }

        CheckEntitiesInTrigger();

        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }

        if (teacherNear && !isOpen)
        {
            TryOpen();
        }
    }

    void CheckEntitiesInTrigger()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        playerNear = false;
        teacherNear = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerNear = true;
            }
            if (hit.CompareTag("Teacher"))
            {
                teacherNear = true;
            }
        }
    }

    public void TryOpen()
    {
        if (isOpen) return;
        if (!CheckRequirements()) return;

        OpenDoor();
    }

    void OpenDoor()
    {
        isOpen = true;
        closeTimer = openDuration;

        if (openSound != null)
            audioSource.PlayOneShot(openSound, openSoundVolume);
    }

    void CloseDoor()
    {
        isOpen = false;
    }

    bool CheckRequirements()
    {
        foreach (var req in requirements)
        {
            if (GameManager.Instance != null)
            {
                Counter counter = GameManager.Instance.GetCounter(req.notebookID);
                if (counter == null || counter.currentCount < req.requiredCount)
                    return false;
            }
        }
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 1.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}

[System.Serializable]
public class DoorRequirement
{
    public string notebookID;
    public int requiredCount;
}