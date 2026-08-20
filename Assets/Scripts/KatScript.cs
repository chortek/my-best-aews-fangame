using UnityEngine;
using UnityEngine.AI;

public class KatScript : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float stepDuration = 0.4f;
    public float waitBetweenSteps = 0.8f;
    public float reductionPerNotebook = 0.04f;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip quietPhrase;
    public float footstepVolume = 0.8f;

    [Header("Step Curve")]
    public AnimationCurve stepCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private NavMeshAgent agent;
    private Transform player;
    private AudioSource audioSource;
    private AudioSource phraseSource;

    private bool isMoving = false;
    private float currentStepTimer = 0f;
    private float currentWaitTimer = 0f;
    private float currentWaitBetweenSteps;
    private float currentReductionMultiplier;
    private float initialWaitBetweenSteps;
    private int collectedNotebooks = 0;
    private bool isQuietMode = false;
    private bool isSpeedReset = false;
    private bool footstepPlayed = false;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    void OnEnable()
    {
        if (agent != null && player != null)
        {
            agent.SetDestination(player.position);
            StartMoveStep();
        }
    }

    void OnDisable()
    {
        if (agent != null)
        {
            agent.speed = 0f;
        }
        isMoving = false;
        footstepPlayed = false;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = moveSpeed;
        agent.acceleration = 50f;
        agent.stoppingDistance = 0.5f;
        agent.angularSpeed = 300f;
        agent.autoBraking = false;
        agent.radius = 0.4f;
        agent.height = 1.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.volume = footstepVolume;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 50f;
        audioSource.minDistance = 1f;

        phraseSource = gameObject.AddComponent<AudioSource>();
        phraseSource.spatialBlend = 0f;
        phraseSource.playOnAwake = false;
        phraseSource.volume = 0.8f;

        initialWaitBetweenSteps = waitBetweenSteps;
        currentWaitBetweenSteps = waitBetweenSteps;
        currentReductionMultiplier = reductionPerNotebook;
        lastPosition = transform.position;
        enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        // --- ПРИНУДИТЕЛЬНАЯ ПРОВЕРКА КАСАНИЯ ---
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < 0.5f)
        {
            Debug.Log($"Teacher {gameObject.name} caught player! (distance check)");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            return;
        }


        if (!agent.isOnNavMesh || !agent.hasPath)
        {
            if (isMoving)
            {
                EndMoveStep();
            }
            return;
        }

        agent.SetDestination(player.position);

        CheckStuck();
        TryOpenNearbyDoors();

        if (!isMoving)
        {
            currentWaitTimer += Time.deltaTime;
            if (currentWaitTimer >= currentWaitBetweenSteps)
            {
                StartMoveStep();
            }
        }
        else if (isMoving)
        {
            currentStepTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(currentStepTimer / stepDuration);
            float curveValue = stepCurve.Evaluate(progress);

            agent.speed = moveSpeed * curveValue;
            agent.acceleration = 100f;

            if (!isQuietMode && footstepSounds.Length > 0 && progress >= 0.5f && !footstepPlayed)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip);
                footstepPlayed = true;
            }

            if (progress >= 1f)
            {
                EndMoveStep();
            }
        }
    }

    void CheckStuck()
    {
        if (!isMoving) return;

        float speed = agent.velocity.magnitude;
        float distance = Vector3.Distance(transform.position, lastPosition);

        if (speed < 0.05f && distance < 0.01f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1f)
            {
                agent.ResetPath();
                agent.SetDestination(player.position);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void TryOpenNearbyDoors()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (Collider hit in hits)
        {
            Door door = hit.GetComponent<Door>();
            if (door != null)
            {
                door.TryOpen();
            }
        }
    }

    void StartMoveStep()
    {
        isMoving = true;
        currentWaitTimer = 0f;
        currentStepTimer = 0f;
        footstepPlayed = false;
    }

    void EndMoveStep()
    {
        isMoving = false;
        agent.speed = 0f;
        currentStepTimer = 0f;
        footstepPlayed = false;
    }

    public void UpdateNotebooksCollected(int count)
    {
        collectedNotebooks = count;

        // ЕСЛИ ТИХИЙ РЕЖИМ — НЕ МЕНЯЕМ ПАУЗУ
        if (isQuietMode)
        {
            Debug.Log($"Quiet mode active, wait locked at {currentWaitBetweenSteps}");
            return;
        }

        float reduction = collectedNotebooks * currentReductionMultiplier;
        currentWaitBetweenSteps = Mathf.Max(initialWaitBetweenSteps - reduction, 0.05f);

        Debug.Log($"Notebooks: {collectedNotebooks}, Wait: {currentWaitBetweenSteps}");

        if (collectedNotebooks >= 13 && !isSpeedReset)
        {
            EnterQuietMode();
        }
    }

    void EnterQuietMode()
    {
        isQuietMode = true;
        isSpeedReset = true;

        if (quietPhrase != null && phraseSource != null)
        {
            phraseSource.PlayOneShot(quietPhrase);
        }

        // ПРИНУДИТЕЛЬНО 3 СЕКУНДЫ
        currentWaitBetweenSteps = 3f;

        isMoving = false;
        agent.speed = 0f;
        currentStepTimer = 0f;
        currentWaitTimer = 0f;
        footstepPlayed = false;

        Debug.Log($"Quiet mode: wait FORCED to {currentWaitBetweenSteps}");
    }

    public void OnSpoopy() { }
    public void OnChase() { }
}