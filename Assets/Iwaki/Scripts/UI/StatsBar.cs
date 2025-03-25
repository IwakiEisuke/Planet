using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] PlayerStats stats;
    [SerializeField] float barSpeed = 10f;

    private void Update()
    {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, stats.GetRatio(), barSpeed * Time.deltaTime);
    }
}
