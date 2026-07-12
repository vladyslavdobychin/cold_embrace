using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform cameraTransform;
    public CharacterController controller;
    private Vector2 moveInput;
    private float yVelocity;

    void Awake() => controller = GetComponent<CharacterController>();

    void OnMove(InputValue movementValue) => moveInput = movementValue.Get<Vector2>();

    // Update is called once per frame
    void Update()
    {
        yVelocity = controller.isGrounded ? -1f : yVelocity + Physics.gravity.y * Time.deltaTime;

        Vector3 move = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * new Vector3(moveInput.x, 0, moveInput.y);
        move.y = yVelocity;

        controller.Move(move * speed * Time.deltaTime);
    }
}
