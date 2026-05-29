using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Teacher : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;
    public MemoryGameManager gameManager;

    [Header("Скорости")]
    public float normalSpeed = 3f;
    public float chaseSpeed = 6f;

    [Header("Обзор")]
    public float visionRange = 10f;
    public float visionAngle = 60f;

    [Header("Цель")]
    public int targetScore = 4; // сколько тетрадей нужно для финала

    private NavMeshAgent agent;
    private bool isActive = false;
    private bool isFinal = false;
    private Vector3 lastSeenPosition;
    private float currentSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        if (gameManager == null || player == null) return;

        // Активация при счёте >= 1
        if (!isActive && gameManager.GetScore() >= 1)
        {
            Activate();
        }

        if (!isActive) return;

        // Погоня при достижении цели
        if (!isFinal && gameManager.GetScore() >= targetScore)
        {
            isFinal = true;
            currentSpeed = chaseSpeed;
            agent.speed = chaseSpeed;
            Debug.Log("Цель достигнута! Учитель переходит в режим погони");
        }

        if (isFinal)
        {
            // Всегда идёт к игроку
            agent.SetDestination(player.position);
        }
        else
        {
            // Поиск + ускорение при виде
            bool canSee = CanSeePlayer();

            if (canSee)
            {
                currentSpeed = chaseSpeed;
                agent.speed = chaseSpeed;
                lastSeenPosition = player.position;
                agent.SetDestination(player.position);
            }
            else
            {
                if (currentSpeed != normalSpeed)
                {
                    currentSpeed = normalSpeed;
                    agent.speed = normalSpeed;
                }

                if (Vector3.Distance(transform.position, lastSeenPosition) > 1f)
                    agent.SetDestination(lastSeenPosition);
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > visionRange) return false;

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > visionAngle / 2) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, visionRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
        lastSeenPosition = player.position;
        Debug.Log("Учитель активирован");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 forward = transform.forward * visionRange;
        Vector3 left = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;
        Vector3 right = Quaternion.Euler(0, visionAngle / 2, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}