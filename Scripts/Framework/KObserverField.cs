
using System;

public class KObserverField<T>
{
    private T _preValue;
    private Action<T,T> _callback;

    public KObserverField(T initValue, Action<T,T> callback)
    {
        _preValue = initValue;
        _callback = callback;
    }

    public void Update(T arg)
    {
        if (_preValue.Equals(arg))
        {
            _callback(_preValue, arg);
            _preValue = arg;
        }
    }
}
