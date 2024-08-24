using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : StateMachine
{
    public GridMap CurMap;
    public float NormalSpeed;
    public float LadderSpeed;

    private GameModel m_gameModel;

 
    // Start is called before the first frame update
    void Start()
    {
        m_gameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        TransitionToState(new PlayerNormalState(this));
    }
    protected override void Update()
    {
        if (m_gameModel.IsPaused)
            return;
        base.Update();
    }
}
