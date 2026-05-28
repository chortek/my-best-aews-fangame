using UnityEngine;

public class MemoryGameStarter : MonoBehaviour
{
    public float range = 3f;
    public MemoryGameManager manager;

    void Update()
    {
        if (manager == null) return;

        // Находим все тетради с тегом "Notebook"
        GameObject[] notebooks = GameObject.FindGameObjectsWithTag("Notebook");
        GameObject nearest = null;
        float nearestDist = range;

        foreach (GameObject nb in notebooks)
        {
            if (!nb.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, nb.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = nb;
            }
        }

        if (nearest != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed, opening notebook: " + nearest.name);
            if (manager.memoryGameUI != null)
                manager.memoryGameUI.SetActive(true);
            manager.StartGame(nearest);
        }
        Debug.Log("Найдено тетрадей: " + notebooks.Length + ", ближайшая: " + (nearest != null ? nearest.name : "null"));
    }
}