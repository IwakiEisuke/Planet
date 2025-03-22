using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float max = 10;
    [SerializeField] float regeneration = 2;

    float _current;

    public bool IsRegenerate { get; set; }
    public float Value { get => _current; set => _current = Mathf.Min(value, max); }

    private void Start()
    {
        _current = max;

        if (regeneration != 0)
        {
            IsRegenerate = true;
        }
    }

    public void Update()
    {
        if (IsRegenerate && _current < max)
        {
            Value += regeneration * Time.deltaTime;
        }
    }

    public float GetRatio()
    {
        return _current / max;
    }
}