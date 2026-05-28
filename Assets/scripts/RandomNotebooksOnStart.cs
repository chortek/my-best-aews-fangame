using UnityEngine;
using System.Collections.Generic;

public class RandomNotebooksOnStart : MonoBehaviour
{
    [Header("Настройки")]
    public int notebooksToKeep = 3;           // сколько тетрадей оставить
    public bool destroyInsteadOfDisable = true; // true - удалить, false - отключить

    [Header("Авто-поиск тетрадей")]
    public string notebookTag = "Notebook";    // тег тетрадей (должен быть присвоен)

    private List<GameObject> allNotebooks = new List<GameObject>();

    void Start()
    {
        // Найти все объекты с тегом "Notebook"
        allNotebooks.Clear();
        GameObject[] found = GameObject.FindGameObjectsWithTag(notebookTag);
        allNotebooks.AddRange(found);

        if (allNotebooks.Count == 0)
        {
            Debug.LogWarning("Не найдено тетрадей с тегом '" + notebookTag + "'");
            return;
        }

        // Ограничить количество (не больше чем есть)
        int toRemove = allNotebooks.Count - notebooksToKeep;
        if (toRemove <= 0)
        {
            Debug.Log("Тетрадей и так меньше или равно " + notebooksToKeep + ", ничего не удаляем");
            return;
        }

        // Перемешиваем список
        List<GameObject> shuffled = new List<GameObject>(allNotebooks);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            GameObject temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        // Оставляем первые notebooksToKeep, остальные удаляем/отключаем
        for (int i = notebooksToKeep; i < shuffled.Count; i++)
        {
            GameObject nb = shuffled[i];
            if (destroyInsteadOfDisable)
                Destroy(nb);
            else
                nb.SetActive(false);
        }

        Debug.Log($"Оставлено тетрадей: {notebooksToKeep} из {allNotebooks.Count}");
    }
}