using UnityEngine;

public class ItemBase : MonoBehaviour, IHoldable
{
    [SerializeField] Transform _targetPivot;

    public bool isBuried;

    private void Start()
    {
        if (isBuried)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
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
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
