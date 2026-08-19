using UnityEngine;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [Header("Движение")]
    public Vector3 openOffset = new Vector3(2f, 0f, 0f);
    public float speed = 3f;
    public float openDuration = 5f;

    [Header("Зона взаимодействия")]
    public float triggerRadius = 1.5f;
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

    [Header("Звук")]
    public AudioClip openSound;

    [Header("Условия")]
    public List<DoorRequirement> requirements;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private float closeTimer = 0f;
    private bool playerNear = false;
    private AudioSource audioSource;

    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (isOpen)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f) isOpen = false;
        }

        CheckPlayerInTrigger();

        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }
    }

    void CheckPlayerInTrigger()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);
        playerNear = false;
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerNear = true;
                break;
            }
        }
    }

    void TryOpen()
    {
        if (isOpen) return;
        if (!CheckRequirements()) return;

        isOpen = true;
        closeTimer = openDuration;

        // ЗВУК ОТКРЫТИЯ
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }
    }

    bool CheckRequirements()
    {
        foreach (var req in requirements)
        {
            // TODO: подключить счётчики
        }
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}

[System.Serializable]
public class DoorRequirement
{
    public string notebookID;
    public int requiredCount;
}