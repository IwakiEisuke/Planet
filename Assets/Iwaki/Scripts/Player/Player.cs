using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5;
    [SerializeField] float accel = 20;
    [SerializeField] float airAccel = 5;

    [SerializeField] float directionTargetMaxDistance = 5;
    [SerializeField] Transform playerDirectionTarget;

    private float PlayerDirectionNormalized { get => (playerDirectionTarget.position - transform.position).x / directionTargetMaxDistance; }

    [Header("Jump")]
    [SerializeField] float jumpHeight = 2;

    [Header("Check Grounded")]
    [SerializeField] LayerMask groundedLayer;
    [SerializeField] float rayDistance = 1;
    [SerializeField] float canGroundedAngle = 45;

    [Header("Crouch")]
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Sliding")]
    [SerializeField] float slidingSpeed = 10;
    [SerializeField] float slidingAngle = 30;

    [Header("Hanging")]
    [SerializeField] Vector2 _ledgeHangingOrigin = new(0, 0.5f);
    [SerializeField] float _ledgeHangingWidth = 0.5f;
    [SerializeField] float _ledgeHangingHeight = 0.2f;

    [Header("Stats")]
    [SerializeField] PlayerStats _oxygen;
    [SerializeField] PlayerStats _health;

    [Header("Item")]
    [SerializeField] ItemBase _item;
    [SerializeField] float _itemSearchRadius = 1f;
    [SerializeField] LayerMask _itemLayer;
    [SerializeField] Joint2D _handJoint;
    [SerializeField] float _throwForce;
    [SerializeField] float _throwAngle;

    public ItemBase HandItem => _item;

    [Header("Breath")]
    [SerializeField] float _consumeOxygenWhenStopBreath = 2;
    [SerializeField] float _toxicDamage = 5;

    Rigidbody2D _rb;

    float _currentSpeed;
    float _currentAccel;

    Vector2 _input;
    bool _isGrounded;
    bool _isCrouching;
    bool _isHanging;

    public bool inToxicField;
    bool _isStopBreath;

    bool _isDead;

    readonly HashSet<Collider2D> _inContacts = new();

    public event Action OnDead;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _currentSpeed = walkSpeed;
        _currentAccel = accel;
    }

    private void Update()
    {
        if (_isDead) return;

        if (!_isHanging)
        {
            IsHanging();
            UpdateSpeed();
            _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, _input.x * _currentSpeed, _currentAccel * Time.deltaTime);
            playerDirectionTarget.localPosition = new Vector2(_rb.linearVelocityX / walkSpeed * directionTargetMaxDistance, 0);
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }

        if (inToxicField && !_isStopBreath)
        {
            if (!_health.Reduce(_toxicDamage * Time.deltaTime))
            {
                _isDead = true;
                OnDead?.Invoke();
                GetComponent<PlayerInput>().enabled = false;
                print("Dead");
            }
        }

        if (_isStopBreath)
        {
            if (!_oxygen.Reduce(_consumeOxygenWhenStopBreath * Time.deltaTime))
            {
                _isStopBreath = false;
            }
        }
    }

    void IsHanging()
    {
        if (_rb.linearVelocityY > 0) return;

        var origin = _ledgeHangingOrigin + (Vector2)transform.position;
        var crossPoint = new Vector2(origin.x + _ledgeHangingWidth * PlayerDirectionNormalized, origin.y);

        var wallHit = Physics2D.Raycast(origin, Vector2.right * PlayerDirectionNormalized, _ledgeHangingWidth, groundedLayer.value);
        var floorHit = Physics2D.Raycast(crossPoint + Vector2.up * _ledgeHangingHeight, Vector2.down, _ledgeHangingHeight, groundedLayer.value);

        if (floorHit.distance > 0.01f && wallHit.collider && floorHit.collider)
        {
            print($"Hanging <wall:{wallHit.collider.name}> <floor:{floorHit.collider.name}>");
            _isHanging = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IsGrounded(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IsGrounded(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _inContacts.Remove(collision.collider);

        if (_inContacts.Count == 0)
        {
            _isGrounded = false;
        }
    }

    void IsGrounded(Collision2D collision)
    {
        // ‚±‚ê‚¢‚éH
        //var hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundedLayer.value);
        //print("Hit: " + hit.collider.name);

        //if (hit.collider == null)
        //{
        //    _isGrounded = false;
        //    return;
        //}

        _inContacts.Add(collision.collider);

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

        if (_isHanging)
        {
            _isHanging = false;
            _rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else if (!_isGrounded) return;

        if (_isCrouching)
        {
            // HeadSliding
            if (_oxygen.Reduce(2.5f))
            {
                var angle = slidingAngle * Mathf.Deg2Rad;
                _rb.linearVelocity = new Vector2(PlayerDirectionNormalized * slidingSpeed * Mathf.Cos(angle), slidingSpeed * Mathf.Sin(angle));
            }
        }
        else
        {
            if (_oxygen.Reduce(1.6f))
            {
                _rb.linearVelocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            }
        }
    }

    void OnCrouch(InputValue value)
    {
        print("Crouch");

        _isCrouching = value.isPressed;
    }

    void UpdateSpeed()
    {
        if (_isGrounded)
        {
            _currentAccel = accel;
        }

        if (!_isGrounded)
        {
            _currentAccel = airAccel;
        }
        else if (_isCrouching)
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

        if (_item == null)
        {
            var hit = Physics2D.OverlapCircle(transform.position, _itemSearchRadius, _itemLayer.value);
            if (!hit) return;

            if (hit.GetComponentInParent<ItemBase>() is ItemBase item)
            {
                if (item.isBuried)
                {
                    item.Dig();
                }

                _item = item;

                var itemRb = _item.GetComponent<Rigidbody2D>();
                var layers = itemRb.excludeLayers.value;
                layers |= LayerMask.GetMask("Player");
                itemRb.excludeLayers = layers;

                _handJoint.connectedBody = itemRb;
                print("PickUp " + _item.name);
            }
            else if (hit.TryGetComponent<ItemPit>(out var pit))
            {
                StartCoroutine(pit.Dig(1));
            }
        }
        else if (_isCrouching)
        {
            DropItem();
        }
        else
        {
            Throw();
        }
    }

    void OnStopBreath()
    {
        print("StopBreath");

        _isStopBreath = true;
    }

    void OnBreathe()
    {
        print("Breathe");

        _isStopBreath = false;
    }

    void Throw()
    {
        print("Throw");

        if (_item)
        {
            var itemRb = _item.GetComponent<Rigidbody2D>();

            DropItem();

            var throwForce = _throwForce * new Vector2(Mathf.Cos(_throwAngle * Mathf.Deg2Rad), Mathf.Sin(_throwAngle * Mathf.Deg2Rad));
            Debug.DrawRay(transform.position, throwForce, Color.white, 10);
            itemRb.AddForce(throwForce);
        }
    }

    void OnEat(InputValue value)
    {
        print("Eat");

        if (_item)
        {
            if (_item.TryGetComponent<IEdible>(out var edible))
            {
                edible.Eat();
                _handJoint.connectedBody = null;
                _item = null;
            }
        }
    }

    void DropItem()
    {
        var itemRb = _item.GetComponent<Rigidbody2D>();
        var layers = itemRb.excludeLayers.value;
        layers &= ~LayerMask.GetMask("Player");
        itemRb.excludeLayers = layers;

        print("Drop " + _item.name);

        _handJoint.connectedBody = null;
        _item = null;
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40
        };

        GUILayout.BeginVertical();
        GUILayout.Label($"_isGrounded {_isGrounded}", style);
        GUILayout.Label($"_isCrouching {_isCrouching}", style);

        foreach (var c in _inContacts)
        {
            GUILayout.Label($"contact: {c.name}", style);
        }

        GUILayout.EndVertical();
    }

    private void OnDrawGizmos()
    {
        var dir = PlayerDirectionNormalized;

        var origin = _ledgeHangingOrigin + (Vector2)transform.position;
        var crossPoint = new Vector2(origin.x + _ledgeHangingWidth * dir, origin.y);

        Debug.DrawLine(origin, crossPoint);
        Debug.DrawLine(crossPoint, crossPoint + _ledgeHangingHeight * Vector2.up);
    }
}
