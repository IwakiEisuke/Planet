using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Last : MonoBehaviour
{
    public UnityEvent EventOnTriggerEnter;
    public UnityEvent EventOnTriggerExit;
    public UnityEvent OnEndingPre;
    public UnityEvent OnEnding;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EventOnTriggerEnter.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EventOnTriggerExit.Invoke();
    }

    public async void Ending()
    {
        print("Ending");

        OnEndingPre.Invoke();

        await Task.Delay(1000);

        OnEnding.Invoke();
    }
}
