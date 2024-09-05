using MyPackage;
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
    Animator anim;
    Animator alertanim;

    public EnemyEatFoodState(BaseEnemy owner, Vector3 start, Vector3 end, GameObject food)
    {
        this.owner = owner;
        StartPos = start;
        EndPos = end;
        this.food = food;
        slice = food.GetComponent<SliceController>();
        anim = owner.transform.GetChild(0).GetComponent<Animator>();
        alertanim = owner.transform.GetChild(1).GetComponent<Animator>();
        
    }
    public override void OnEnter()
    {
        alertanim.SetBool("isalert", true);
        anim.SetBool("iseating", false);
        owner.StartCoroutine(MoveFromTo());
    }

    public override void OnExit()
    {
        owner.StopAllCoroutines();
        owner.EnemyMotor.StopAllCoroutines();
        anim.SetBool("iseating", false);
        alertanim.SetBool("isalert", false);
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
        anim.SetBool("iseating", true);
        yield return owner.EnemyMotor.MoveTo(owner.CurMap.GetPointViaPosition(EndPos));
        
        if (!slice.IsPickup)
        {
            EventSystem.Instance.SendEvent<SliceEatEvent>(typeof(SliceEatEvent), new SliceEatEvent(food.transform.position, food.GetComponent<SliceController>().CurSliceType));
            GameObject.Destroy(food);
        }
        
        owner.TransitionToState(new EnemyRandomState(owner));
    }
}
