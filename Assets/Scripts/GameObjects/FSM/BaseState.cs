using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected StateMachine m_owner;
    public void SetOwner(StateMachine owner)
    {
        m_owner = owner;
    }
    public abstract void OnEnter();
    public abstract void OnStart();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
