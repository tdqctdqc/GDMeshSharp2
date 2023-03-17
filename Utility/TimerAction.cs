
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

    public TimerAction(float timerPeriod)
    {
        _timerPeriod = timerPeriod;
        _timer = timerPeriod;
    }

    public void Process(float delta)
    {
        ProcessAction(delta, _action);
    }

    public bool ProcessAction(float delta, Action action)
    {
        _timer += delta;
        if (_timer >= _timerPeriod)
        {
            _timer = 0f;
            action.Invoke();
            return true;
        }

        return false;
    }

    public void ResetTimer()
    {
        _timer = 0f;
    }
}
