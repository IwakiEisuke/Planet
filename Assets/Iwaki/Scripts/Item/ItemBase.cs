using UnityEngine;

public class ItemBase : MonoBehaviour, IHoldable
{
    [SerializeField] Transform _targetPivot;

    public void OnDrop()
    {
        _targetPivot = null;
    }

    public void OnPickUp(Transform hand)
    {
        _targetPivot = hand;
    }
}
