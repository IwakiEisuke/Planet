using UnityEngine;
using UnityEngine.Events;

public class Last : MonoBehaviour
{
    public UnityEvent EventOnTriggerEnter;
    public UnityEvent EventOnTriggerExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EventOnTriggerEnter.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EventOnTriggerExit.Invoke();
    }
}
