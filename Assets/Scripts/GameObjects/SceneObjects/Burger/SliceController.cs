using MyPackage;
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
    public float SlicePlaceOffset = SushiController.SLICE_HEIGHT/3.0f;
    public SliceType CurSliceType;

    public Vector3 RefreshPosition;
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
    private bool isPickup;
    public bool IsPickup
    {
        get => isPickup;
    }
    void Start()
    {
        Initialize();

        Point curpoint = GridMap.Instance.GetPointViaPosition(transform.position);

        transform.position = GridMap.Instance.GetPositionViaPoint(curpoint) - Vector3.up * SlicePlaceOffset;
        RefreshPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Initialize()
    {
        isSet = false;
    }
    public void Initialize(SushiGenerationController generation)
    {

    }

    public void SetPiece(Transform piece)
    {
        for (int i = 0; i < Pieces.Length; i++)
        {
            if(Pieces[i] == piece)
            {
                Pieces[i].GetChild(0).localPosition = new Vector3(Pieces[i].GetChild(0).localPosition.x,
                                                      Pieces[i].GetChild(0).localPosition.y - (MaxPieceDownDistance - PieceDownFactor * m_stepCount * MaxPieceDownDistance));
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
    public void FallDown(bool isHitByOther = false)
    {
        if (isFalling || isSet)
            return;
        m_stepCount = 0;
        foreach (var item in Pieces)
        {
            item.GetChild(0).localPosition = new Vector3(item.GetChild(0).localPosition.x, 0, 0);
        }
        //transform.position -= Vector3.down * MaxPieceDownDistance;

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
                fallDownHandler = StartCoroutine(StartFallDown(targetPos, isHitByOther));
                return;
            }
            //If slice is placed on a ladder, then it should land on a ladder grid that has platform neighbour
            else if((curMap.GetPointState(curPoint)&GridState.Ladder)>0)
            {
                if((curMap.GetPointState(new Point(curPoint.X - 1,curPoint.Y)) & GridState.None) > 0
                || (curMap.GetPointState(new Point(curPoint.X + 1, curPoint.Y)) & GridState.None) > 0)
                {
                    Vector3 targetPos = curMap.GetPositionViaPoint(curPoint) - Vector3.up * SlicePlaceOffset;
                    fallDownHandler = StartCoroutine(StartFallDown(targetPos, isHitByOther));
                    return;
                }
            }
        }
        //If can't find a platform, means it will reach the plate, link all the 
        //slices to the plate, and plate will give all the slices a proper position
        //to finally be placed
        SushiController Sushi = findSushi();
        fallDownHandler = StartCoroutine(StartFallDownToSushi(Sushi.GetSlicePos(),Sushi));
        isSet = true;
        EventSystem.Instance.SendEvent(typeof(SliceSetEvent), new SliceSetEvent(transform.position, CurSliceType, isHitByOther));
    }

    //Called by player after pick up this slice, it will restore the state of slice
    public void PickUp()
    {
        isPickup = true;
        m_stepCount = 0;
        foreach (var item in Pieces)
        {
            item.GetComponent<PieceController>().Initialize();
            item.GetChild(0).localPosition = new Vector3(item.GetChild(0).localPosition.x, 0, 0);

            item.GetComponent<BoxCollider2D>().enabled = false;
        }
        
    }
    public void DropDown()
    {
        isPickup = false;
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
            Color origin = item.GetChild(0).GetComponent<SpriteRenderer>().color;
            if (isTransparent)
            {
                
                item.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(origin.r,origin.g,origin.b, 0.5f);

            }
            else
            {
                item.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(origin.r, origin.g, origin.b, 1f);

            }
        }
    }

    SushiController findSushi()
    {
        var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down, 100.0f, LayerMask.GetMask("Sushi"));
        return hit.collider.transform.GetComponent<SushiController>();
    }

    IEnumerator StartFallDown(Vector3 target, bool isHitByOther = false)
    {
        EventSystem.Instance.SendEvent<SliceFallEvent>(typeof(SliceFallEvent), new SliceFallEvent(transform.position,target,CurSliceType,isHitByOther));
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
                hit.collider.transform.parent.GetComponent<SliceController>().FallDown(true);
                Bounce(target);
            }
            yield return new WaitForEndOfFrame();
        }
        transform.position = target;
        isFalling = false;
    }
    IEnumerator StartFallDownToSushi(Vector3 target, SushiController Sushi)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            if (transform.position.y < -20)
            {
                Destroy(gameObject);
            }
            transform.Translate(Vector3.down * Time.deltaTime * FallDownSpeed);
            //Check whether collide with other slice
            var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - Vector2.up * SushiController.SLICE_HEIGHT / 2,
                Vector2.down, 0.1f, LayerMask.GetMask("Piece"));
            if (hit)
            {
                hit.collider.transform.parent.GetComponent<SliceController>().FallDown();
                Bounce(target);
            }
            yield return new WaitForEndOfFrame();
        }
        transform.position = target;
        transform.SetParent(Sushi.transform);
        isFalling = false;
        Sushi.SetSlice(CurSliceType);
    }

    //Slice will bounce if collide with other slices
    void Bounce(Vector3 target)
    {
        if (IsSet)
            return;
        StartCoroutine(BounceAnim());
    }
    IEnumerator BounceAnim()
    {
        Pieces[0].localPosition = new Vector3(Pieces[0].localPosition.x, 0, 0) + Vector3.up * 0.2f;
        Pieces[1].localPosition = new Vector3(Pieces[1].localPosition.x, 0, 0) + Vector3.up * 0.1f;
        Pieces[2].localPosition = new Vector3(Pieces[2].localPosition.x, 0, 0) + Vector3.up * 0f;
        Pieces[3].localPosition = new Vector3(Pieces[3].localPosition.x, 0, 0) + Vector3.up * 0.2f;
        yield return new WaitForSecondsRealtime(0.2f);
        Pieces[0].localPosition = new Vector3(Pieces[0].localPosition.x, 0, 0) + Vector3.up * 0.4f;
        Pieces[1].localPosition = new Vector3(Pieces[1].localPosition.x, 0, 0) + Vector3.up * 0.2f;
        Pieces[2].localPosition = new Vector3(Pieces[2].localPosition.x, 0, 0) + Vector3.up * 0.1f;
        Pieces[3].localPosition = new Vector3(Pieces[3].localPosition.x, 0, 0) + Vector3.up * 0.3f;
        yield return new WaitForSecondsRealtime(0.2f);
        Pieces[0].localPosition = new Vector3(Pieces[0].localPosition.x, 0, 0) + Vector3.up * 0.2f;
        Pieces[1].localPosition = new Vector3(Pieces[1].localPosition.x, 0, 0) + Vector3.up * 0.1f;
        Pieces[2].localPosition = new Vector3(Pieces[2].localPosition.x, 0, 0) + Vector3.up * 0f;
        Pieces[3].localPosition = new Vector3(Pieces[3].localPosition.x, 0, 0) + Vector3.up * 0.2f;
        yield return new WaitForSecondsRealtime(0.2f);
        Pieces[0].localPosition = new Vector3(Pieces[0].localPosition.x,0,0);
        Pieces[1].localPosition =new Vector3(Pieces[1].localPosition.x, 0, 0);
        Pieces[2].localPosition =new Vector3(Pieces[2].localPosition.x, 0, 0);
        Pieces[3].localPosition =new Vector3(Pieces[3].localPosition.x, 0, 0);
    }

}
