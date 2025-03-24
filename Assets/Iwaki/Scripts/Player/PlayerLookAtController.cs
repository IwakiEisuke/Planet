using System.Linq;
using UnityEngine;

public class PlayerLookAtController : MonoBehaviour
{
    [SerializeField] float checkRadius;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform head;

    [SerializeField] Transform _lookAt;
    
    public Transform LookAt => _lookAt;

    private void Update()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, checkRadius, layerMask.value);

        hits = hits.OrderByDescending(x => Vector2.Distance(x.transform.position, head.position)).ToArray();

        var _isExistInterestingStuff = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent<ItemPit>(out var pit))
            {
                _lookAt = pit.transform;
                _isExistInterestingStuff = true;
            }
            else if (hits[i].GetComponentInParent<ItemBase>() is ItemBase item)
            {
                _lookAt = item.transform;
                _isExistInterestingStuff = true;
            }
        }

        if (!_isExistInterestingStuff)
        {
            _lookAt = null;
        }
    }
}
