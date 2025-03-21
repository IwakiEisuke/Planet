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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Switch Camera");
        cam.Prioritize();
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
