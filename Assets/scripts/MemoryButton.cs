using UnityEngine;
using UnityEngine.UI;

public class MemoryButton : MonoBehaviour
{
    public int index;
    public MemoryGameManager manager;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (manager != null)
                    manager.OnButtonClick(index);
                else
                    Debug.LogError("MemoryButton: manager не назначен");
            });
        }
    }
}