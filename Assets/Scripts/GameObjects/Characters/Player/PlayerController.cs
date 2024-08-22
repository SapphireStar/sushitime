using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameModel m_gamemodel;
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        m_gamemodel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
    }

    // Update is called once per frame
    void Update()
    {
        StepSlice();
    }

    void StepSlice()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - Vector2.up * 0.5f, Vector2.up,0.1f,LayerMask.GetMask("Piece"));
        if(hit)
        {
            hit.collider.GetComponent<PieceController>().StepOn();
        }
    }
    void detectEnemy()
    {

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
    }
}
