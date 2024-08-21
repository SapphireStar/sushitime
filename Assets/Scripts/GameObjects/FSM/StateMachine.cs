using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private BaseState m_curState;
    private BaseState m_nextState;
    
    void Start()
    {
        
    }


    public void TransitionToState(BaseState state)
    {
        m_nextState = state;

        if(m_curState!=null)
            m_curState.OnExit();

        m_nextState.OnEnter();
        m_curState = m_nextState;
        m_nextState = null;
    }
    
    protected virtual void Update()
    {
        if(m_curState!=null)
        {
            m_curState.OnUpdate();
        }
        
    }
}
