using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : StateMachine
{
    public GridMap CurMap;
    public float NormalSpeed;
    public float LadderSpeed;
    public GridBasedMovement EnemyMotor;
    public float EatCD = 10;
    public float CurCD = 1;

    GameModel gameModel;
    // Start is called before the first frame update
    void Start()
    {
        EnemyMotor = GetComponent<GridBasedMovement>();
        //in the RapidMove coroutine, move speed is splited into horizontal and vertical
        //set them seperately according to NormalSpeed and LadderSpeed
        EnemyMotor.HorizontalSpeed = NormalSpeed;
        EnemyMotor.VerticalSpeed = LadderSpeed;

        gameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        gameModel.PropertyValueChanged += onGameModelChanged;
    }
    public virtual void Initalize()
    {
        TransitionToState(new EnemyRandomState(this));
    }
    protected override void Update()
    {
        base.Update();
    }
    private void OnDestroy()
    {
        gameModel.PropertyValueChanged -= onGameModelChanged;

    }
    void onGameModelChanged(object sender, PropertyValueChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "PlayerDead":
                StopAllCoroutines();
                break;
            default:
                break;
        }
    }
}
