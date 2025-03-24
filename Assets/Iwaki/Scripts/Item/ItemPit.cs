using System.Collections;
using UnityEngine;

public class ItemPit : MonoBehaviour
{
    [SerializeField] GameObject _buriedItem;
    [SerializeField] ParticleSystem _particleSystem;

    public IEnumerator Dig(float time)
    {
        _particleSystem.Play();
        yield return new WaitForSeconds(time);
        Instantiate(_buriedItem, transform.position, transform.rotation);
        _particleSystem.Stop();
        Destroy(gameObject);
    }

    [ContextMenu("AAA")]
    private void Test_Dig()
    {
        StartCoroutine(Dig(1));
    }

    private void OnDrawGizmos()
    {
        var split = 32;

        var p = new Vector3[split];

        var radius = 0.5f;
        for (int i = 0; i < split; i++)
        {
            var theta = Mathf.PI * 2 * i / split;
            p[i] = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta)) * radius + transform.position;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLineStrip(p, true);
    }
}
