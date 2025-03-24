using System.Linq;
using UnityEngine;

public class PlayerLookAtController : MonoBehaviour
{
    [SerializeField] float checkRadius;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform head;
    [SerializeField] Player player;

    [Space(10)]
    [SerializeField] Transform lookAt;

    public Transform LookAt => lookAt;
    public LookAtType LookAtType { get; private set; }

    private void Update()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, checkRadius, layerMask.value);

        hits = hits.OrderByDescending(x => Vector2.Distance(x.transform.position, head.position)).ToArray();

        var _isExistInterestingStuff = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent<ItemPit>(out var pit))
            {
                lookAt = pit.transform;
                LookAtType = LookAtType.ItemPit;
                _isExistInterestingStuff = true;
            }
            else if (hits[i].GetComponentInParent<ItemBase>() is ItemBase item)
            {
                if (item != player.HandItem)
                {
                    lookAt = item.transform;
                    LookAtType = LookAtType.Item;
                    _isExistInterestingStuff = true;
                }
            }
        }

        if (!_isExistInterestingStuff)
        {
            lookAt = null;
        }
    }

    public float DistanceFromHead()
    {
        if (lookAt)
        {
            return Vector2.Distance(lookAt.position, head.position);
        }
        else
        {
            return float.MaxValue;
        }
    }
}

public enum LookAtType
{
    Item,
    ItemPit,
}
