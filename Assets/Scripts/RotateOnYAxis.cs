using UnityEngine;

public class RotateOnYAxis : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 45f;

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}
