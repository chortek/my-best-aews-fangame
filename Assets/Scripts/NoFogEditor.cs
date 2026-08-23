#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class DisableFogInEditor : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        // Принудительно выключаем туман в редакторе
        RenderSettings.fog = false;
        Debug.Log("DisableFogInEditor: туман выключен в редакторе");
#endif
    }
}