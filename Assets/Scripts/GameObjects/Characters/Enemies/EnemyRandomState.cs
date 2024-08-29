using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRandomState : BaseState
{
    List<Point> availablePoints;
    Point target;
    BaseEnemy owner;
    

    Coroutine moveHandle;
    public EnemyRandomState(BaseEnemy owner)
    {
        this.owner = owner;
    }
    public override void OnEnter()
    {


        availablePoints = new List<Point>();
        availablePoints.AddRange(owner.CurMap.GetPointsByState(GridState.None));
        availablePoints.AddRange(owner.CurMap.GetPointsByState(GridState.Ladder));
        //availablePoints.AddRange(owner.CurMap.GetPointsByState(GridState.Walkable by enemy)); allow enemy to walk on somewhere player can't
        moveHandle = owner.StartCoroutine(RandomMove());
       
    }

    public override void OnExit()
    {
        owner.StopAllCoroutines();
        owner.EnemyMotor.StopAllCoroutines();
    }

    public override void OnStart()
    {
    }

    public override void OnUpdate()
    {
        var hit = Physics2D.OverlapCircle(owner.transform.position, 2,LayerMask.GetMask("Piece"));
        if (hit)
        {
            Transform sushi = hit.transform.parent;
            Vector3 start = sushi.GetChild(0).position;
            Vector3 end = sushi.GetChild(3).position;
            owner.TransitionToState(new EnemyEatFoodState(owner, start, end, sushi.gameObject));
        }
    }

    IEnumerator RandomMove()
    {
        while(true)
        {
            Debug.Log("Move to");
            yield return owner.EnemyMotor.MoveTo(availablePoints[UnityEngine.Random.Range(0, availablePoints.Count)]);
        }
    }
}
