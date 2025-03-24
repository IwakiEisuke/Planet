using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float max = 10;
    [SerializeField] float regeneration = 2;
    [SerializeField] float restartRegen = 1.2f;
    [SerializeField] bool canRegenerate;

    float _current;
    float _timeBeforeValueDecreased;

    public event Action OnExhaustion;

    public bool IsRegenerate { get; set; }

    private void Start()
    {
        _current = max;

        if (regeneration != 0)
        {
            IsRegenerate = true;
        }
    }

    public void Add(float value)
    {
        _current += value;

        if (_current > max)
        {
            _current = max;
        }
    }

    public bool Reduce(float value)
    {
        _current -= value;

        IsRegenerate = false;
        _timeBeforeValueDecreased = 0;

        if (_current <= 0)
        {
            _current = 0;
            OnExhaustion?.Invoke();
            return false;
        }

        return true;
    }

    public void Update()
    {
        if (canRegenerate)
        {
            _timeBeforeValueDecreased += Time.deltaTime;

            if (IsRegenerate && _current < max)
            {
                Add(regeneration * Time.deltaTime);
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