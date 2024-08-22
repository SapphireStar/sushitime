using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceController : MonoBehaviour
{
    public BurgerController Parent;
    public Transform[] Pieces;
    public float PieceDownFactor = 0.1f;
    public float MaxPieceDownDistance = 0.1f;
    public float FallDownSpeed = 2;
    public float SlicePlaceOffset = 0.125f;


    private float m_stepCount;
    private Coroutine fallDownHandler;
    //Doesn't allow player step on silices when it is falling
    private bool isFalling;
    public bool IsFalling
    {
        get => isFalling;
    }
    //Doesn't allow player step on silices when it is set on the tray
    private bool isSet;
    public bool IsSet
    {
        get => isSet;
    }
    void Start()
    {
        Initialize();
        Parent = transform.parent.GetComponent<BurgerController>();

        Point curpoint = GridMap.Instance.GetPointViaPosition(transform.position);

        transform.position = GridMap.Instance.GetPositionViaPoint(curpoint) - Vector3.up * SlicePlaceOffset;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Initialize()
    {
        isSet = false;
    }

    public void SetPiece(Transform piece)
    {
        for (int i = 0; i < Pieces.Length; i++)
        {
            if(Pieces[i] == piece)
            {
                Pieces[i].localPosition = new Vector3(Pieces[i].localPosition.x,
                                                      Pieces[i].localPosition.y - (MaxPieceDownDistance - PieceDownFactor * m_stepCount * MaxPieceDownDistance));
                ++m_stepCount;
                if (m_stepCount >= 4)
                {


                    FallDown();

                }
                break;
            }
        }

    }

    //
    public void FallDown()
    {


        if (isFalling)
            return;
        m_stepCount = 0;
        foreach (var item in Pieces)
        {
            item.localPosition = new Vector3(item.localPosition.x, 0, 0);
        }
        transform.position -= Vector3.down * MaxPieceDownDistance;

        isFalling = true;
        foreach (var item in Pieces)
        {
            item.GetComponent<PieceController>().Initialize();
        }

        GridMap curMap = GridMap.Instance;
        Point curPoint = curMap.GetPointViaPosition(transform.position);
        while(curPoint.Y>=0)
        {
            curPoint = new Point(curPoint.X, curPoint.Y - 1);
            if((curMap.GetPointState(curPoint) & GridState.None) > 0)
            {
                Vector3 targetPos = curMap.GetPositionViaPoint(curPoint) - Vector3.up * SlicePlaceOffset;
                fallDownHandler = StartCoroutine(StartFallDown(targetPos));
                return;
            }
            //If slice is placed on a ladder, then it should land on a ladder grid that has platform neighbour
            else if((curMap.GetPointState(curPoint)&GridState.Ladder)>0)
            {
                if((curMap.GetPointState(new Point(curPoint.X - 1,curPoint.Y)) & GridState.None) > 0
                || (curMap.GetPointState(new Point(curPoint.X + 1, curPoint.Y)) & GridState.None) > 0)
                {
                    Vector3 targetPos = curMap.GetPositionViaPoint(curPoint) - Vector3.up * SlicePlaceOffset;
                    fallDownHandler = StartCoroutine(StartFallDown(targetPos));
                    return;
                }
            }
        }
        //If can't find a platform, means it will reach the plate, link all the 
        //slices to the plate, and plate will give all the slices a proper position
        //to finally be placed
        fallDownHandler = StartCoroutine(StartFallDownLocal(Parent.GetSlicePos(this)));
        isSet = true;
    }

    

    IEnumerator StartFallDown(Vector3 target)
    {
        while(Vector3.Distance(transform.position,target)>0.1f)
        {
            transform.Translate(Vector3.down * Time.deltaTime * FallDownSpeed);
            //Check whether collide with other slice
            var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - Vector2.up * BurgerController.SLICE_HEIGHT / 2, 
                Vector2.down, 0.1f, LayerMask.GetMask("Piece"));
            if(hit)
            {
                hit.collider.transform.parent.GetComponent<SliceController>().FallDown();
                Bounce(target);
            }
            yield return new WaitForEndOfFrame();
        }
        transform.position = target;
        isFalling = false;
    }
    IEnumerator StartFallDownLocal(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.1f)
        {
            transform.Translate(Vector3.down * Time.deltaTime * FallDownSpeed);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = target;
        isFalling = false;
    }

    //Slice will bounce if collide with other slices
    void Bounce(Vector3 target)
    {

    }

}
