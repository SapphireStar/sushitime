using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRandomState : BaseState
{
    List<Point> availablePoints;
    Point target;
    BaseEnemy owner;
    Animator anim;


    Coroutine moveHandle;
    public EnemyRandomState(BaseEnemy owner)
    {
        this.owner = owner;
        anim = owner.transform.GetChild(0).GetComponent<Animator>();
    }
    public override void OnEnter()
    {
        anim.SetBool("iswalking", true);

        owner.CurCD = owner.EatCD;
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
        checkIfEat();
    }
    void checkIfEat()
    {
        owner.CurCD -= Time.deltaTime;
        if(owner.CurCD <= 0)
        {
            owner.CurCD = owner.EatCD;
            var hit = Physics2D.OverlapCircle(owner.transform.position, 50, LayerMask.GetMask("Piece"));
            if (hit)
            {
                Transform sushi = hit.transform.parent;
                SliceController slice = sushi.GetComponent<SliceController>();
                if((slice.CurSliceType == SliceType.Slice2||
                    slice.CurSliceType == SliceType.Slice3 ||
                    slice.CurSliceType == SliceType.Slice4)&&
                    !slice.IsSet)
                {
                    Vector3 start = sushi.GetChild(0).position;
                    Vector3 end = sushi.GetChild(3).position;
                    owner.TransitionToState(new EnemyEatFoodState(owner, start, end, sushi.gameObject));
                }

            }
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
