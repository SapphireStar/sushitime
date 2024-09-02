using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbLadderState : BaseState
{
    GameModel gamemodel;
    PlayerMovement owner;
    bool isLeftNearPlatform;
    bool isRightNearPlatform;
    Animator anim;
    public PlayerClimbLadderState(PlayerMovement owner)
    {
        this.owner = owner;
        anim = owner.transform.GetChild(0).GetComponent<Animator>();
    }
    public override void OnEnter()
    {
        //Switch animator state
        //Set player X-axis align with ladder X-axis
        Debug.Log("Start Climb Ladder");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);
        owner.transform.position = pointPos;

        ModelManager.Instance.GetModel<GameModel>(typeof(GameModel)).IsClimbing = true;
    }

    public override void OnExit()
    {
        anim.SetBool("iswalking", false);
    }

    public override void OnStart()
    {
    }

    public override void OnUpdate()
    {
        handleMovement();
        checkIsNearPlatform();
        handleGetOffLadder();
    }

    void handleMovement()
    {
        float verticalDir = Input.GetAxisRaw("Vertical");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);

        if(verticalDir != 0)
        {
            anim.SetBool("iswalking", true);
        }
        else
        {
            anim.SetBool("iswalking", false);
        }

        if (verticalDir < 0)
        {
            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X, curPoint.Y - 1));
            if (!((curPoint.Y == 0
                || (nextGrid & GridState.Ladder) == 0) // check collide with obstacles
                && owner.transform.position.y <= pointPos.y))
            {
                owner.transform.Translate(Vector3.down * Time.deltaTime * owner.LadderSpeed);
            }
        }
        if (verticalDir > 0)
        {
            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X, curPoint.Y + 1));
            if (!((curPoint.Y == owner.CurMap.StepHeight - 1
                || (nextGrid & GridState.Ladder) == 0) // check collide with obstacles
                && owner.transform.position.y >= pointPos.y))
            {
                owner.transform.Translate(Vector3.up * Time.deltaTime * owner.LadderSpeed);
            }
        }

    }

    void checkIsNearPlatform()
    {
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Point leftPoint = new Point(curPoint.X - 1, curPoint.Y);
        Point rightPoint = new Point(curPoint.X + 1, curPoint.Y);
        GridState leftPointState = owner.CurMap.GetPointState(leftPoint);
        GridState rightPointState = owner.CurMap.GetPointState(rightPoint);

        if ((leftPointState & GridState.None) > 0)
        {
            isLeftNearPlatform = true;
        }
        else
        {
            isLeftNearPlatform = false;
        }

        if ((rightPointState & GridState.None) > 0)
        {
            isRightNearPlatform = true;
        }
        else
        {
            isRightNearPlatform = false;
        }
    }

    void handleGetOffLadder()
    {
        float horizontalDir = Input.GetAxisRaw("Horizontal");
        Point curPoint = owner.CurMap.GetPointViaPosition(owner.transform.position);
        Vector3 pointPos = owner.CurMap.GetPositionViaPoint(curPoint);
        if (isLeftNearPlatform && horizontalDir < 0)
        {

            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X - 1, curPoint.Y));
            if (!((curPoint.X == 0
                || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                && owner.transform.position.x <= pointPos.x))
            {
                owner.TransitionToState(new PlayerNormalState(owner));
            }
        }


        if (isRightNearPlatform && horizontalDir > 0)
        {

            GridState nextGrid = owner.CurMap.GetPointState(new Point(curPoint.X + 1, curPoint.Y));
            if (!((curPoint.X == owner.CurMap.StepWidth - 1
                || (nextGrid & GridState.Obstacle) > 0) // check collide with obstacles
                && owner.transform.position.x >= pointPos.x))
            {
                owner.TransitionToState(new PlayerNormalState(owner));
            }
        }

    }


}