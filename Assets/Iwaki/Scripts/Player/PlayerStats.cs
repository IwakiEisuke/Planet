using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float max = 10;
    [SerializeField] float regeneration = 2;
    [SerializeField] float restartRegen = 1.2f;
    [SerializeField] bool canRegenerate;
    [SerializeField] bool successWhenExhausted;

    public UnityEvent OnExhaustion;

    float _current;
    float _timeBeforeValueDecreased;

    readonly public List<Func<float, float, bool>> ConditionsForReduction = new();

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
        if (_current == 0)
        {
            return false;
        }

        var conditions = ConditionsForReduction.Select(f => f.Invoke(_current, value));

        if (conditions.All(b => b == true))
        {
            _current -= value;

            IsRegenerate = false;
            _timeBeforeValueDecreased = 0;

            if (_current <= 0)
            {
                _current = 0;
                OnExhaustion?.Invoke();
                return successWhenExhausted;
            }
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