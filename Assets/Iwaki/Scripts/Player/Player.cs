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

    [Header("MovementDirection")]
    [SerializeField] float directionDistance = 5;
    [SerializeField] float directionMinZ = -0.05f;
    [SerializeField] Transform playerDirectionTransform;
    [SerializeField] float playerDirection = 0;
    float playerDirectionRaw = 0;
    private float PlayerDirectionNormalized { get => (playerDirectionTransform.position - transform.position).x / directionDistance; }

    [Header("Jump")]
    [SerializeField] float jumpHeight = 2;

    Vector2 _groundNormal = Vector2.up;

    [Header("Check Grounded")]
    [SerializeField] LayerMask groundedLayer;
    [SerializeField] float rayDistance = 1;
    [SerializeField] float canGroundedAngle = 45;
    [SerializeField] float circleRadius = 0.5f;

    [Header("Crouch")]
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Sliding")]
    [SerializeField] float slidingSpeed = 10;
    [SerializeField] float slidingAngle = 30;

    [Header("Physics")]
    [SerializeField] float walkingFriction = 0.5f;
    [SerializeField] float slidingFriction = 1f;

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

    float _targetSpeed;
    float _currentAccel;

    Vector2 _input;
    bool _isGrounded;
    bool _isCrouching;
    bool _isHanging;
    bool _isSliding;

    public bool inToxicField;
    bool _isStopBreath;

    bool _isDead;

    readonly HashSet<Collider2D> _inContacts = new();

    public event Action OnDead;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _targetSpeed = walkSpeed;
        _currentAccel = accel;
    }

    private void Update()
    {
        if (_isDead) return;

        if (!_isHanging && !_isCrouching)
        {
            playerDirection = Mathf.MoveTowards(playerDirection, playerDirectionRaw, walkSpeed * Time.deltaTime);
        }

        if (!_isHanging)
        {
            IsHanging();
            UpdateSpeed();

            if (!_isSliding)
            {
                if (_isGrounded)
                {
                    var hit = Physics2D.CircleCast(transform.position, circleRadius, Vector2.down, 5, groundedLayer.value);
                    var dir = Quaternion.FromToRotation(Vector2.up, hit.normal) * (Vector2.right * _input.x);
                    _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, dir * _targetSpeed, _currentAccel * Time.deltaTime);
                    Debug.DrawRay(hit.point, dir);
                }
                else
                {
                    _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, _input.x * _targetSpeed, _currentAccel * Time.deltaTime);
                }
            }

            var leftDirection = new Vector3(-directionDistance, 0, directionMinZ);
            var rightDirection = new Vector3(directionDistance, 0, directionMinZ);

            playerDirectionTransform.localPosition = Vector3.Slerp(leftDirection, rightDirection, (playerDirection + 1) / 2);
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
        var existFloor = false;
        var isGrounded = false;
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y < 0)
            {
                continue;
            }
            else
            {
                existFloor = true;
            }

            if (Vector2.Dot(Vector2.up, contact.normal) >= Mathf.Cos(canGroundedAngle * Mathf.Deg2Rad))
            {
                isGrounded = true;
                _groundNormal = contact.normal;
            }
        }

        if (existFloor)
        {
            _isGrounded = isGrounded;
        }

        _inContacts.Add(collision.collider);
    }

    void OnMove(InputValue value)
    {
        print("Move");

        _input = value.Get<Vector2>();

        if (Mathf.Abs(_input.x) > 0)
        {
            playerDirectionRaw = _input.x > 0 ? 1 : -1;
        }
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
                _rb.linearVelocity = new Vector2(playerDirectionRaw * slidingSpeed * Mathf.Cos(angle), slidingSpeed * Mathf.Sin(angle));
                _rb.sharedMaterial.friction = slidingFriction;
                _isSliding = true;
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

        if (!_isCrouching)
        {
            _rb.sharedMaterial.friction = walkingFriction;
            _isSliding = false;
        }
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
            _targetSpeed = crouchSpeed;
        }
        else
        {
            _targetSpeed = walkSpeed;
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
        GUILayout.Label($"{nameof(_isSliding)} {_isSliding}", style);
        GUILayout.Label($"friction {_rb.sharedMaterial.friction}", style);

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
