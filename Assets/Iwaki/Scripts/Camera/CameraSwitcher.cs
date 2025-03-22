using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase cam;
    [SerializeField] BoxCollider2D box;

    private void Reset()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        print($"Enter Area:{name}");

        if (collision.GetComponent<Player>() is not Player player) return;

        var p = player.transform.position;
        var max = box.bounds.max;
        var min = box.bounds.min;

        if (min.x < p.x && p.x < max.x && min.y < p.y && p.y < max.y)
        {
            print("Switch Camera");
            cam.Prioritize();
        }
    }

    private void OnDrawGizmos()
    {
        var max = box.bounds.max;
        var min = box.bounds.min;

        var p = new Vector3[] { new(max.x, max.y), new(max.x, min.y), new(min.x, min.y), new(min.x, max.y) };

        Gizmos.color = Color.magenta;
        Gizmos.DrawLineStrip(p, true);
    }
}
