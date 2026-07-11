using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Vector3 offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() => offset = transform.position - target.position;

    void LateUpdate() => transform.position = target.position + offset;
}
