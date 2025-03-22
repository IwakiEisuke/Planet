using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float speed;
    Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void OnMove(InputValue value)
    {
        print("Move");

        var input = value.Get<Vector2>();

        _rb.linearVelocityX = input.x * speed;
    }

    void OnJump(InputValue value)
    {
        print("Jump");

    }

    void OnCrouch(InputValue value)
    {
        print("Crouch");

    }

    void OnInteract(InputValue value)
    {
        print("Interact");

    }

    void OnBreathe(InputValue value)
    {
        print("Breath");

    }

    void OnThrow(InputValue value)
    {
        print("Throw");

    }

    void OnEat(InputValue value)
    {
        print("Eat");

    }
}
