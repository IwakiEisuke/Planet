using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [HideInInspector]
    public CheckPointManager manager;
    public int id;

    private void Start()
    {
        manager = FindAnyObjectByType<CheckPointManager>();
    }

    public void SetCheckPoint()
    {
        manager.SetCheckPoint(id);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.layer | LayerMask.GetMask("Player")) > 0)
        {
            SetCheckPoint();
        }
    }
}
