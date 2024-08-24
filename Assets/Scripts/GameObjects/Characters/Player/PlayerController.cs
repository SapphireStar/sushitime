using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameModel m_gamemodel;
    private bool isInPickedup;
    private bool isOnPlate = true;
    private bool isAwayOtherSlice;
    private bool canDropSlice;
    //Original parent of the picked up slice
    private Transform sliceParent;
    private Transform sliceTransform;
    //restore slice pos after player died
    private Vector3 sliceLastPos;
    //record curSushiPlate Position
    private Vector3 curSushiPlate;
    void Start()
    {
        Initialize();
        m_gamemodel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        m_gamemodel.PropertyValueChanged += gameModelHandler;

    }

    void Initialize()
    {
        isInPickedup = false;
    }
    private void OnDestroy()
    {
        m_gamemodel.PropertyValueChanged -= gameModelHandler;
    }
    void gameModelHandler(object sender,PropertyValueChangedEventArgs e )
    {
        switch (e.PropertyName)
        {
            case "PlayerDead":
                restoreSlicePos();
                Initialize();
                break;
            default:
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(m_gamemodel.IsPaused)
        {
            return;
        }
        stepSlice();
        detectPlate();
        detectOtherSlice();
        decideIsCanDrop();
        if (Input.GetKeyDown(KeyCode.Z))
        {
            pickDropSlice();
        }
    }

    void stepSlice()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - Vector2.up * 0.5f, Vector2.up,0.1f,LayerMask.GetMask("Piece"));
        if(hit)
        {
            hit.collider.GetComponent<PieceController>().StepOn();
        }
    }
    void pickDropSlice()
    {
        //prohibit player pickup during climbing
        if(m_gamemodel.IsClimbing)
        {
            return;
        }
        if (!isInPickedup)
        {

            //Use transform.right, so when player turn around, the pickup direction is always where player facing to
            //Use to checkpoint to prevent allow player pickup on the ladder
            var hit = Physics2D.Raycast(transform.position - new Vector3(0, 0.2f,0), new Vector2(transform.localScale.x,0), 1, LayerMask.GetMask("Piece"));

            if (hit)
            {
                sliceTransform = hit.collider.transform.parent;
                //do not pickup when slice is falling
                if (sliceTransform.GetComponent<SliceController>().IsFalling)
                    return;

                isInPickedup = true;
                m_gamemodel.IsPickedUp = true;
                sliceLastPos = sliceTransform.position;
                sliceParent = sliceTransform.parent;
                sliceTransform.SetParent(transform);
                sliceTransform.localPosition = new Vector3(0, 1, 0);
                //call pickup to restore the position of pieces
                sliceTransform.GetComponent<SliceController>().PickUp();
            }
        }
        else if(canDropSlice)
        {
            isInPickedup = false;
            m_gamemodel.IsPickedUp = false;
            Point point = GridMap.Instance.GetPointViaPosition(new Vector3(curSushiPlate.x, transform.position.y, transform.position.z));
            Vector3 pos = GridMap.Instance.GetPositionViaPoint(point) - new Vector3(0,SushiController.SLICE_HEIGHT/6.0f,0);
            sliceTransform.position = pos;
            sliceTransform.SetParent(sliceParent);
            sliceTransform.GetComponent<SliceController>().DropDown();
            sliceTransform = null;
        }
    }
    void restoreSlicePos()
    {
        isInPickedup = false;
        m_gamemodel.IsPickedUp = false;
        if(sliceTransform!=null)
        {
            sliceTransform.position = sliceLastPos;
            sliceTransform.SetParent(sliceParent);
            sliceTransform.GetComponent<SliceController>().DropDown();
            sliceTransform.GetComponent<SliceController>().SetTransparent(false);
            sliceTransform = null;
        }
    }

    //If no available Sushi plate under player, not allow player to drop the slice
    void detectPlate()
    {
        var hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down, 100.0f, LayerMask.GetMask("Sushi"));
        if (hit)
        {
            curSushiPlate = hit.collider.transform.position;
            isOnPlate = true;
        }
        else
        {
            isOnPlate = false;
        }
    }
    //make sure slice won't collapse with other slices
    void detectOtherSlice()
    {
        var hit = Physics2D.OverlapCircle(transform.position, 0.45f, LayerMask.GetMask("Piece"));
        if(hit)
        {
            isAwayOtherSlice = false;
        }
        else
        {
            isAwayOtherSlice = true;
        }
    }

    //Decide if player can drop slice according to plate and other slices
    void decideIsCanDrop()
    {
        if (isAwayOtherSlice && isOnPlate)
        {
            canDropSlice = true;
            if (sliceTransform != null)
                sliceTransform.GetComponent<SliceController>().SetTransparent(false);
        }
        else
        {
            canDropSlice = false;
            if (sliceTransform != null)
                sliceTransform.GetComponent<SliceController>().SetTransparent(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            m_gamemodel.PlayerDead = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position - Vector3.up * 0.5f, transform.position - Vector3.up * 0.6f);

        Gizmos.DrawLine(transform.position - new Vector3(0, 0.2f, 0), transform.position - new Vector3(0, 0.2f, 0) + Vector3.right);

    }
}
