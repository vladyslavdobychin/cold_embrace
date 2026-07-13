using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private LayerMask groundMask;

    void Update()
    {
        Ray ray = aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector3 lookPoint = hit.point;
            lookPoint.y = transform.position.y;
            transform.LookAt(lookPoint);
        }
    }
}
