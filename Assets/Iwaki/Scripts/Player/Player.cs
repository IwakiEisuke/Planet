using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5;
    [SerializeField] float accel = 20;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 2;

    [Header("Check Grounded")]
    [SerializeField] LayerMask groundedLayer;
    [SerializeField] float rayDistance = 1;
    [SerializeField] float canGroundedAngle = 45;

    [Header("Crouch")]
    [SerializeField] float crouchSpeed = 2.5f;

    Rigidbody2D _rb;

    float _currentSpeed;

    Vector2 _input;
    bool _isGrounded;
    bool _isCrouching;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _currentSpeed = walkSpeed;
    }

    private void Update()
    {
        _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, _input.x * _currentSpeed, accel * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IsGrounded(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IsGrounded(collision);
    }

    void IsGrounded(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector2.Dot(Vector2.up, contact.normal) >= Mathf.Cos(canGroundedAngle * Mathf.Deg2Rad))
            {
                _isGrounded = true;
            }
        }
    }

    void OnMove(InputValue value)
    {
        print("Move");

        _input = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        print("Jump");

        if (!_isGrounded) return;

        var hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundedLayer.value);
        if (hit)
        {
            print("Hit : " + hit.collider.name);
            _rb.linearVelocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            _isGrounded = false;
        }
        else
        {
            print("NotGrounded");
        }
    }

    void OnCrouch(InputValue value)
    {
        print("Crouch");

        _isCrouching = value.isPressed;

        if (value.isPressed)
        {
            _currentSpeed = crouchSpeed;
        }
        else
        {
            _currentSpeed = walkSpeed;
        }
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
