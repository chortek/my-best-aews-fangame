using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform target; // Игрок (перетащить в инспекторе)

    void LateUpdate()
    {
        if (target == null) return;

        // Направление к игроку (только по X и Z, Y не учитываем)
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Убираем наклон

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}