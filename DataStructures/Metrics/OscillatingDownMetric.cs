
using Godot;

public class OscillatingDownMetric : IMetric<float>
{
    private float _period, _max, _min, _shrinkFactor;

    public OscillatingDownMetric(float period, float max, float min, float shrinkFactor)
    {
        _shrinkFactor = shrinkFactor;
        _period = period;
        _max = max;
        _min = min;
    }

    public float GetMetric(float t)
    {
        if (t == 0f) return _max;
        var v = (Mathf.Sin(t * Mathf.Pi * 2f / _period) * (_max - _min) + (_max - _min) + _min) / (t * _shrinkFactor);
        return Mathf.Min(_max, v);
    }
}
