using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalState : BaseState
{
    PlayerMovement owner;
    bool isNearLadder;
    GameModel m_gamemodel;
    public PlayerNormalState(PlayerMovement owner)
    {
        this.owner = owner;
    }
    public override void OnEnter()
    {
        //change animator state
        //Set player Y-axis align with ladder X-axis
        Debug.Log("Start Walk on Platform");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);
        owner.transform.position = pointPos;
        m_gamemodel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        m_gamemodel.IsClimbing = false;
    }

    public override void OnExit()
    {
    }

    public override void OnStart()
    {
    }

    public override void OnUpdate()
    {
        handleMovement();
        checkIsNearLadder();
        handleClimbLadder();
    }

    void handleMovement()
    {
        float horizontalDir = Input.GetAxisRaw("Horizontal");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);

        //when player not out of boundaries, or not collide with obstacles, stop moving
        if (horizontalDir < 0)
        {
            owner.transform.localScale = new Vector3(-1,1,1);
            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X - 1, curPoint.Y));
            if (!((curPoint.X == 0
                || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                && owner.transform.position.x <= pointPos.x))
            {
                owner.transform.Translate(Vector3.left * Time.deltaTime * owner.NormalSpeed);
            }

        }
        else if (horizontalDir > 0)
        {
            owner.transform.localScale = new Vector3(1, 1, 1);
            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X + 1, curPoint.Y));
            if (!((curPoint.X == owner.CurMap.StepWidth - 1
                || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                && owner.transform.position.x >= pointPos.x))
            {
                owner.transform.Translate(Vector3.right * Time.deltaTime * owner.NormalSpeed);
            }
        }
    }

    bool checkIsNearLadder()
    {
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        GridState curPointState = owner.CurMap.GetPointState(curPoint);
        if ((curPointState & GridState.Ladder)>0)
        {
            isNearLadder = true;
            return true;
        }
        else
        {
            isNearLadder = false;
            return false;
        }
    }

    //Used to detect whether player can climb the ladder and transition to climb state
    void handleClimbLadder()
    {
/*        //If player picked up a slice, prohibit player 
        if (m_gamemodel.IsPickedUp)
            return;*/
        float verticalDir = Input.GetAxisRaw("Vertical");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);
        if (isNearLadder)
        {
            if(verticalDir<0)
            {
                GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X, curPoint.Y - 1));
                if (!((curPoint.Y == 0
                    || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                    && owner.transform.position.y <= pointPos.y))
                {
                    owner.TransitionToState(new PlayerClimbLadderState(owner));
                }
            }
            if (verticalDir > 0)
            {
                GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X, curPoint.Y + 1));
                if (!((curPoint.Y == owner.CurMap.StepHeight - 1
                    || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                    && owner.transform.position.y >= pointPos.y))
                {
                    owner.TransitionToState(new PlayerClimbLadderState(owner));
                }
            }
        }
    }
}
