using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : StateMachine
{
    public GridMap CurMap;
    public float NormalSpeed;
    public float LadderSpeed;

    private bool is_paused;

 
    // Start is called before the first frame update
    void Start()
    {
        TransitionToState(new PlayerNormalState(this));
    }

}
