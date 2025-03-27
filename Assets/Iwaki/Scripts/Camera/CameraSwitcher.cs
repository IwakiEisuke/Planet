using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] BoxCollider2D box;
    [SerializeField] bool fitColliderBoundsToCameraView;

    static CinemachineCamera current;

    private void Reset()
    {
        box = GetComponent<BoxCollider2D>();
    }

    [ContextMenu(nameof(FitBoundToView))]
    private void FitBoundToView()
    {
        var height = cam.Lens.OrthographicSize * 2;
        var width = height * cam.Lens.Aspect;

        box.size = new Vector2(width, height);
        box.transform.position = new Vector2(cam.transform.position.x, cam.transform.position.y);
    }

    private void Update()
    {
        if (fitColliderBoundsToCameraView && !Application.isPlaying)
        {
            FitBoundToView();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (current == cam) return;
        if (collision.GetComponent<Player>() is not Player player) return;

        var p = player.transform.position;
        var max = box.bounds.max;
        var min = box.bounds.min;

        if (min.x < p.x && p.x < max.x && min.y < p.y && p.y < max.y)
        {
            print($"Enter Area:{name}");
            SetCurrent();
        }
    }

    public void SetCurrent()
    {
        cam.Prioritize();
        current = cam;
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
