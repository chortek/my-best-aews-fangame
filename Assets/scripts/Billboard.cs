using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // Получаем текущие углы поворота объекта
            Vector3 currentRotation = transform.localEulerAngles;

            // Переносим угол поворота Z от камеры (или рассчитываем угол на нее)
            // Вариант А: Объект повторяет наклон камеры (для 2D/2.5D игр)
            currentRotation.z = mainCameraTransform.eulerAngles.z;

            // Применяем измененный поворот, оставляя X и Y нетронутыми
            transform.localEulerAngles = currentRotation;
        }
    }
}
