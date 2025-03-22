using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 5;
    [SerializeField] float accel = 20;
    Rigidbody2D _rb;

    Vector2 _input;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, _input.x * speed, accel * Time.deltaTime);
    }

    void OnMove(InputValue value)
    {
        print("Move");

        _input = value.Get<Vector2>();
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
