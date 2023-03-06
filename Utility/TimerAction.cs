
using System;

public class TimerAction
{
    private float _timerPeriod, _timer;
    private Action _action;

    public TimerAction(float timerPeriod, float timer, Action action)
    {
        _timerPeriod = timerPeriod;
        _timer = timer;
        _action = action;
    }

    public void Process(float delta)
    {
        _timer += delta;
        if (_timer >= _timerPeriod)
        {
            _timer = 0f;
            _action?.Invoke();
        }
    }
}
