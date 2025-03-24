using UnityEngine;

public class PlayerExpression : MonoBehaviour
{
    [SerializeField] PlayerLookAtController lookAtController;
    [SerializeField] GameObject expression;

    private void Update()
    {
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
