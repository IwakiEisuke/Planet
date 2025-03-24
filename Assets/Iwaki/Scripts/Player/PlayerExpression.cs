using UnityEngine;

public class PlayerExpression : MonoBehaviour
{
    [SerializeField] PlayerLookAtController lookAtController;
    [SerializeField] GameObject expression;
    [SerializeField] float maxDistance;

    private void Update()
    {
        if (lookAtController.DistanceFromHead() > maxDistance)
        {
            expression.SetActive(false);
            return;
        }

        if (lookAtController.LookAtType == LookAtType.ItemPit)
        {
            expression.SetActive(true);
        }
        else
        {
            expression.SetActive(false);
        }
    }
}
