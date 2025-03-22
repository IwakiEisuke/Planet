using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] string statsName;
    [SerializeField] float max = 10;
    [SerializeField] float regenerate = 2;
    float _current;

    public float Value => _current;

    public void Update()
    {
        if (_current < max)
        {
            _current = Mathf.Min(_current + regenerate * Time.deltaTime, max);
        }
    }

    public void Reduce(float value)
    {
        _current -= value;
    }

    public void Add(float value)
    {
        _current = Mathf.Min(_current + value, max);
    }

    public float GetRatio()
    {
        return _current / max;
    }
}