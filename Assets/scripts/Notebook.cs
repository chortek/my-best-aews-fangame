using UnityEngine;

public class Notebook : MonoBehaviour
{
    public MemoryGameManager manager;
    public float interactRange = 2f;

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("Notebook: игрок с тегом Player не найден");
    }

    void Update()
    {
        if (player == null || manager == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            manager.memoryGameUI.SetActive(true);
            manager.StartGame(gameObject);
        }
    }
}