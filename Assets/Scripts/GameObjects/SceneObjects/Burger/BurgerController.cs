using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurgerController : MonoBehaviour
{
    public Transform[] Pieces;
    public float PieceDownFactor = 0.9f;
    public float MaxPieceDownDistance = 0.1f;
    public float FallDownSpeed = 2;


    private float m_stepCount;
    private Coroutine fallDownHandler;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetPiece(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetPiece(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetPiece(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetPiece(3);
        }
    }

    public void SetPiece(int i)
    {
        Pieces[i].localPosition = new Vector3(Pieces[i].localPosition.x,
                                              Pieces[i].localPosition.y - (MaxPieceDownDistance - PieceDownFactor * m_stepCount));
        ++m_stepCount;
        if(m_stepCount >=3)
        {
            m_stepCount = 0;
            fallDown();
            foreach (var item in Pieces)
            {
                item.localPosition = Vector3.zero;
            }
        }
    }

    //
    void fallDown()
    {
        GridMap curMap = GridMap.Instance;
        Point curPoint = curMap.GetPointViaPosition(transform.position);
        while(curPoint.Y>=0)
        {
            curPoint = new Point(curPoint.X, curPoint.Y - 1);
            if(curMap.GetPointState(curPoint) == GridState.None)
            {
                Vector3 targetPos = curMap.GetPositionViaPoint(curPoint) - Vector3.down * 0.375f;
                fallDownHandler = StartCoroutine(StartFallDown(targetPos));
                return;
            }
        }
        //If can't find a platform, means it will reach the plate, link all the 
        //slices to the plate, and plate will give all the slices a proper position
        //to finally be placed
        
    }

    IEnumerator StartFallDown(Vector3 target)
    {
        while(Vector3.Distance(transform.position,target)>0.1f)
        {
            transform.Translate(Vector3.down * Time.deltaTime * FallDownSpeed);
            yield return new WaitForEndOfFrame();
        }
        transform.position = target;
    }
}
