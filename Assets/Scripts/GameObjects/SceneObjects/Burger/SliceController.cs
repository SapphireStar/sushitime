using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceController : MonoBehaviour
{
    public SushiController Parent;
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
        if (isFalling || isSet)
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
        fallDownHandler = StartCoroutine(StartFallDown(findSushi()));
        isSet = true;
    }

    //Called by player after pick up this slice, it will restore the state of slice
    public void PickUp()
    {
        m_stepCount = 0;
        foreach (var item in Pieces)
        {
            item.GetComponent<PieceController>().Initialize();
            item.localPosition = new Vector3(item.localPosition.x, 0, 0);

            item.GetComponent<BoxCollider2D>().enabled = false;
        }
        
    }
    public void DropDown()
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),transform.localScale.y,1);
        foreach (var item in Pieces)
        {
            item.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
    public void SetTransparent(bool isTransparent)
    {
        foreach (var item in Pieces)
        {
            Color origin = item.GetComponent<SpriteRenderer>().color;
            if (isTransparent)
            {
                
                item.GetComponent<SpriteRenderer>().color = new Color(origin.r,origin.g,origin.b, 0.5f);

            }
            else
            {
                item.GetComponent<SpriteRenderer>().color = new Color(origin.r, origin.g, origin.b, 1f);

            }
        }
    }

    Vector3 findSushi()
    {
        var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down, 100.0f, LayerMask.GetMask("Sushi"));
        return hit.collider.GetComponent<SushiController>().GetSlicePos();
    }

    IEnumerator StartFallDown(Vector3 target)
    {
        while(Vector3.Distance(transform.position,target)>0.1f)
        {
            if(transform.position.y<-10)
            {
                Destroy(gameObject);
            }
            transform.Translate(Vector3.down * Time.deltaTime * FallDownSpeed);
            //Check whether collide with other slice
            var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - Vector2.up * SushiController.SLICE_HEIGHT / 2, 
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

    //Slice will bounce if collide with other slices
    void Bounce(Vector3 target)
    {

    }

}
