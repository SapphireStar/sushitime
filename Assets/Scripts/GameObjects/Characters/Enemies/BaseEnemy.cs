using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : StateMachine
{
    public GridMap CurMap;
    public float NormalSpeed;
    public float LadderSpeed;
    public GridBasedMovement EnemyMotor;
    public Stack<Point> curRoute;

    private bool is_paused;
    // Start is called before the first frame update
    void Start()
    {
        EnemyMotor = GetComponent<GridBasedMovement>();
        //in the RapidMove coroutine, move speed is splited into horizontal and vertical
        //set them seperately according to NormalSpeed and LadderSpeed
        EnemyMotor.HorizontalSpeed = NormalSpeed;
        EnemyMotor.VerticalSpeed = LadderSpeed;

        TransitionToState(new EnemyRandomState(this));
    }
    protected override void Update()
    {
        base.Update();
    }

}
