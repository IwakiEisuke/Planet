using System.Threading.Tasks;
using UnityEngine;

public class ItemBase : MonoBehaviour, IHoldable
{
    [SerializeField] Transform _targetPivot;

    public bool isBuried;

    Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (isBuried)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void OnDrop()
    {
        _targetPivot = null;
    }

    public void OnPickUp(Transform hand)
    {
        
        _targetPivot = hand;
    }

    public void Dig()
    {
        isBuried = false;
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public async void Release(float t)
    {
        await Task.Delay((int)(t * 1000));
        _rb.excludeLayers &= ~LayerMask.GetMask("Player");
    }
}
