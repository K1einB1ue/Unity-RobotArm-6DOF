

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KStateMachine
{
    public bool loop;
    private int _index;
    private List<Action> _actions = new();
    private List<Func<bool>> _stateTrans = new();
    private bool _acted = false;
    
    public void Add(Action action,Func<bool> stateTran)
    {
        _actions.Add(action);
        _stateTrans.Add(stateTran);
    }
    
    
    public void Update()
    {

        if (_index == _stateTrans.Count)
        {
            if (!loop) return;
            _index = 0;
        }
        if (!_acted)
        {
            _actions[_index]?.Invoke();
            _acted = true;
        }
        if (!_stateTrans[_index].Invoke()) return;
        _index++;
        _acted = false;
    }

}
