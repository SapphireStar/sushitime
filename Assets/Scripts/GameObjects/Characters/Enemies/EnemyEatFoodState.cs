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
    
    public EnemyEatFoodState(BaseEnemy owner, Vector3 start, Vector3 end, GameObject food)
    {
        this.owner = owner;
        StartPos = start;
        EndPos = end;
        this.food = food;
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
       
    }
    IEnumerator MoveFromTo()
    {
        yield return owner.EnemyMotor.MoveTo(owner.CurMap.GetPointViaPosition(StartPos));
        yield return owner.EnemyMotor.MoveTo(owner.CurMap.GetPointViaPosition(EndPos));
        SliceController slice = food.GetComponent<SliceController>();
        if (!slice.IsPickup)
        {
            GameObject.Destroy(food);
        }
        
        owner.TransitionToState(new EnemyRandomState(owner));
    }
}
