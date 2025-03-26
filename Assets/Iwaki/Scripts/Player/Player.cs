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
    [SerializeField] float turnSpeed = 6;
    float playerDirectionRaw = 0;
    private float PlayerDirectionNormalized { get => (playerDirectionTransform.position - transform.position).x / directionDistance; }

    [Header("Jump")]
    [SerializeField] float jumpHeight = 2;

    bool _canJump;

    [Header("Check Grounded")]
    [SerializeField] LayerMask groundedLayer;
    [SerializeField] float rayDistance = 1;
    [SerializeField] float canGroundedAngle = 45;
    [SerializeField] float minGroundedRateAngle = 30;
    [SerializeField] float maxGroundedRateAngle = 60;
    [SerializeField] float circleRadius = 0.5f;
    [SerializeField] float groundedCooldown = 0.1f;
    [SerializeField] float coyoteTime = 0.2f;

    Vector2 _recentGroundNormal = Vector2.up;
    float _groundedRate;
    float _canGroundedTimer;
    bool _canGrounded = true;
    float _coyoteTimer;

    [Header("Crouch")]
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Sliding")]
    [SerializeField] float slidingSpeed = 10;
    [SerializeField] float slidingAngle = 30;
    [SerializeField] float cancelSlidingSpeed = 1f;

    [Header("Physics")]
    [SerializeField] float walkingFriction = 0.5f;
    [SerializeField] float slidingFriction = 1f;

    [Header("Hanging")]
    [SerializeField] Vector2 _ledgeHangingOrigin = new(0, 0.5f);
    [SerializeField] float _ledgeHangingWidth = 0.5f;
    [SerializeField] float _ledgeHangingHeight = 0.2f;
    [SerializeField] float _hangingCooldown = 0.3f;
    bool _canHanging;
    float _hangingRestoreTimer;

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
    [SerializeField] float _timeFromThrowingToReleasingExcludeLayer = 0.1f;

    public ItemBase HandItem => _item;

    [Header("Breath")]
    [SerializeField] float _consumeOxygenWhenStopBreath = 2;
    [SerializeField] float _toxicDamage = 5;
    [SerializeField] float _breathValue = 1;

    Rigidbody2D _rb;

    float _targetSpeed;
    float _currentAccel;

    Vector2 _input;
    bool _isGrounded;
    bool _isHalfGrounded;
    bool _isCrouching;
    bool _isHanging;
    bool _isSliding;

    public bool inToxicField;
    bool _isStopBreath;

    bool _isDead;

    readonly HashSet<Collider2D> _inContacts = new();

    public event System.Action OnDead;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _targetSpeed = walkSpeed;
        _currentAccel = accel;

        _oxygen.ConditionsForReduction.Add((current, diff) => _isStopBreath);
    }

    private void Update()
    {
        if (_isDead) return;

        _coyoteTimer -= Time.deltaTime;

        if (!_isHalfGrounded && _coyoteTimer <= 0)
        {
            _canJump = false;
        }

        if (Mathf.Abs(_input.x) > 0 && !_isCrouching)
        {
            playerDirectionRaw = _input.x > 0 ? 1 : -1;
        }

        if (_isSliding && Mathf.Abs(_rb.linearVelocityX) < cancelSlidingSpeed)
        {
            _isSliding = false;
        }

        if (!_canGrounded)
        {
            _canGroundedTimer -= Time.deltaTime;
            if (_canGroundedTimer <= 0)
            {
                _canGrounded = true;
            }
        }

        if (!_canHanging)
        {
            _hangingRestoreTimer -= Time.deltaTime;
            if (_hangingRestoreTimer <= 0)
            {
                _canHanging = true;
            }
        }

        if (!_isHanging)
        {
            IsHanging();
        }

        if (!_isHanging)
        {
            playerDirection = Mathf.MoveTowards(playerDirection, playerDirectionRaw, turnSpeed * Time.deltaTime);
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
            else if (_isGrounded)
            {
                var hit = Physics2D.Raycast(transform.position, Vector2.down, 5, groundedLayer.value);
                var dir = Quaternion.FromToRotation(Vector2.up, hit.normal) * (Vector2.right * playerDirectionRaw);

                var targetVel = _rb.linearVelocity.magnitude * dir;
                _rb.linearVelocity = targetVel;

                Debug.DrawRay(hit.point, dir, Color.yellow);
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
        if (!_canHanging) return;

        if (_rb.linearVelocityY > 0) return;

        var origin = _ledgeHangingOrigin + (Vector2)transform.position;
        var crossPoint = new Vector2(origin.x + _ledgeHangingWidth * PlayerDirectionNormalized, origin.y);

        var wallHit = Physics2D.Raycast(origin, Vector2.right * PlayerDirectionNormalized, _ledgeHangingWidth, groundedLayer.value);
        var floorHit = Physics2D.Raycast(crossPoint + Vector2.up * _ledgeHangingHeight, Vector2.down, _ledgeHangingHeight, groundedLayer.value);

        if (Vector2.Dot(Vector2.up, floorHit.normal) < Mathf.Cos(canGroundedAngle * Mathf.Deg2Rad)) return;

        if (floorHit.distance > 0.01f && wallHit.collider && floorHit.collider)
        {
            print($"Hanging <wall:{wallHit.collider.name}> <floor:{floorHit.collider.name}>");
            _isHanging = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            var leftDirection = new Vector3(-directionDistance, 0, directionMinZ);
            var rightDirection = new Vector3(directionDistance, 0, directionMinZ);

            playerDirectionRaw = (playerDirection > 0 ? 1 : -1);
            playerDirectionTransform.localPosition = Vector3.Slerp(leftDirection, rightDirection, (playerDirectionRaw + 1) / 2);
            playerDirection = playerDirectionRaw;
        }
    }

    void UnHanging()
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _isHanging = false;
        _canHanging = false;
        _hangingRestoreTimer = _hangingCooldown;
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
            _isHalfGrounded = false;
        }
    }

    void IsGrounded(Collision2D collision)
    {
        if (!_canGrounded) return;

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
                _recentGroundNormal = contact.normal;
                Debug.DrawRay(contact.point, contact.normal, Color.blue);
            }

            if (Vector2.Dot(Vector2.up, contact.normal) >= Mathf.Cos(canGroundedAngle * Mathf.Deg2Rad))
            {
                isGrounded = true;
            }

            _groundedRate = Mathf.Clamp01(1 - ((Mathf.Acos(Vector2.Dot(Vector2.up, contact.normal)) * Mathf.Rad2Deg - minGroundedRateAngle) / (maxGroundedRateAngle - minGroundedRateAngle)));
        }

        if (existFloor)
        {
            _isGrounded = isGrounded;

            if (_isGrounded)
            {
                _coyoteTimer = coyoteTime;
            }
        }

        _isHalfGrounded = existFloor;
        _canJump = _isHalfGrounded;
        _inContacts.Add(collision.collider);
    }

    void OnMove(InputValue value)
    {
        print("Move");

        _input = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        print("Jump");

        if (!_isHanging && !_canJump) return;

        if (_isCrouching && !_isHanging && _isGrounded)
        {
            // HeadSliding
            if (_oxygen.Reduce(2.5f))
            {
                var angle = slidingAngle * Mathf.Deg2Rad;
                _rb.linearVelocity = new Vector2(playerDirectionRaw * slidingSpeed * Mathf.Cos(angle), slidingSpeed * Mathf.Sin(angle));
                _rb.sharedMaterial.friction = slidingFriction;
                _isSliding = true;

                _canGroundedTimer = groundedCooldown;
                _canGrounded = false;

                _isGrounded = false;
            }
        }
        else
        {
            if (_oxygen.Reduce(1.6f))
            {
                Vector2 jumpVel;

                if (_isHanging)
                {
                    jumpVel = Vector2.up * Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
                }
                else
                {
                    jumpVel = Vector3.Slerp(_recentGroundNormal, Vector2.up, _groundedRate) * Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (jumpHeight * Mathf.Clamp01(0.7f + _groundedRate)));
                }

                _canGroundedTimer = groundedCooldown;
                _canGrounded = false;

                _rb.linearVelocityX += jumpVel.x;
                _rb.linearVelocityY = jumpVel.y;
            }
        }

        _canJump = false;

        if (_isHanging)
        {
            UnHanging();
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
        else
        {
            UnHanging();
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

        _oxygen.Add(_breathValue);

        _isStopBreath = false;
    }

    void Throw()
    {
        print("Throw");

        if (_item)
        {
            _item.Release(_timeFromThrowingToReleasingExcludeLayer);

            var itemRb = _item.GetComponent<Rigidbody2D>();

            DropItem();

            var throwForce = _throwForce * new Vector2(Mathf.Cos(_throwAngle * Mathf.Deg2Rad) * playerDirectionRaw, Mathf.Sin(_throwAngle * Mathf.Deg2Rad));
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
        GUILayout.Label($"_isHalfGrounded {_isHalfGrounded}", style);
        GUILayout.Label($"_isCrouching {_isCrouching}", style);
        GUILayout.Label($"_isSliding {_isSliding}", style);
        GUILayout.Label($"friction {_rb.sharedMaterial.friction}", style);
        GUILayout.Label($"_groundedRate {_groundedRate}", style);
        GUILayout.Label($"_canJump {_canJump}", style);

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
