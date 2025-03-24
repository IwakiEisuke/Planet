using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] PlayerStats stats;
    [SerializeField] float barSpeed = 0.5f;

    private void Update()
    {
        bar.fillAmount = Mathf.MoveTowards(bar.fillAmount, stats.GetRatio(), barSpeed * Time.deltaTime);
    }
}
