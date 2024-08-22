using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    private SliceController m_parent;
    private bool isStepped;
    // Start is called before the first frame update
    void Start()
    {
        m_parent = transform.parent.GetComponent<SliceController>();
    }
    public void Initialize()
    {
        isStepped = false;
    }
    public void StepOn()
    {
        if(!isStepped && !m_parent.IsFalling && !m_parent.IsSet)
        {
            isStepped = true;
            m_parent.SetPiece(transform);
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
