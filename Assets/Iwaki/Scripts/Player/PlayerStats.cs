using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float max = 10;
    [SerializeField] float regeneration = 2;
    [SerializeField] float restartRegen = 1.2f;
    [SerializeField] bool canRegenerate;

    float _current;
    float _timeBeforeValueDecreased;

    public bool IsRegenerate { get; set; }
    public float Value
    {
        get => _current;
        set
        {
            if (value - _current < 0)
            {
                IsRegenerate = false;
                _timeBeforeValueDecreased = 0;
            }

            _current = Mathf.Min(value, max);
        }
    }

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
        if (canRegenerate)
        {
            _timeBeforeValueDecreased += Time.deltaTime;

            if (IsRegenerate && _current < max)
            {
                Value += regeneration * Time.deltaTime;
            }
            else if (_timeBeforeValueDecreased > restartRegen)
            {
                IsRegenerate = true;
            }
        }
    }

    public float GetRatio()
    {
        return _current / max;
    }
}