using UnityEngine;
using System.Collections.Generic;

public class NotebookSpawner : MonoBehaviour
{
    public List<Group> groups = new List<Group>();

    void Start()
    {
        foreach (var group in groups)
        {
            SpawnGroup(group);
        }
    }

    void SpawnGroup(Group group)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag(group.tagName);

        if (all.Length == 0)
        {
            Debug.LogWarning($"No objects with tag: {group.tagName}");
            return;
        }

        List<GameObject> toRemove = new List<GameObject>();

        foreach (var obj in all)
        {
            if (obj != group.guaranteedNotebook)
                toRemove.Add(obj);
        }

        Shuffle(toRemove);

        int toRemoveCount = all.Length - group.totalCount;
        for (int i = 0; i < toRemoveCount && i < toRemove.Count; i++)
        {
            if (toRemove[i] != null)
                Destroy(toRemove[i]);
        }

        Debug.Log($"{group.tagName}: осталось {GameObject.FindGameObjectsWithTag(group.tagName).Length} шт.");
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    [System.Serializable]
    public class Group
    {
        public string tagName = "Notebook";
        public int totalCount = 17;
        public GameObject guaranteedNotebook;
    }
}