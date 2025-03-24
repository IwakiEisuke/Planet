using UnityEngine;

public interface IHoldable
{
    void OnPickUp(Transform hand);

    void OnDrop();
}
