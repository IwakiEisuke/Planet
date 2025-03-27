using UnityEngine;

public class PlayerToxicReceiver : MonoBehaviour
{
    [SerializeField] Player player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<ToxicGas>())
        {
            player.inToxicField = true;
            if (player.autoStopBreath) player.isStopBreath = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<ToxicGas>())
        {
            player.inToxicField = false;
            if (player.autoStopBreath) player.isStopBreath = false;
        }
    }
}
