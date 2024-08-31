using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyEatFoodState : BaseState
{
    BaseEnemy owner;
    private Vector3 StartPos;
    private Vector3 EndPos;
    private GameObject food;
    SliceController slice;

    public EnemyEatFoodState(BaseEnemy owner, Vector3 start, Vector3 end, GameObject food)
    {
        this.owner = owner;
        StartPos = start;
        EndPos = end;
        this.food = food;
        slice = food.GetComponent<SliceController>();
    }
    public override void OnEnter()
    {
        owner.StartCoroutine(MoveFromTo());
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
        if (slice.IsPickup||slice.IsFalling)
        {
            owner.TransitionToState(new EnemyRandomState(owner));
        }
    }
    IEnumerator MoveFromTo()
    {
        yield return owner.EnemyMotor.MoveTo(owner.CurMap.GetPointViaPosition(StartPos));
        yield return owner.EnemyMotor.MoveTo(owner.CurMap.GetPointViaPosition(EndPos));
        
        if (!slice.IsPickup)
        {
            GameObject.Destroy(food);
        }
        
        owner.TransitionToState(new EnemyRandomState(owner));
    }
}
