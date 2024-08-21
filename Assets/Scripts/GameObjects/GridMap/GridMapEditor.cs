using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMapEditor : MonoBehaviour
{
    public MapData CurMapData;
    public GridMap CurMap;
    public GridState SelectedState;

    private void Awake()
    {
        CurMapData = CurMap.CurMap;
    }
    public void RefreshMap()
    {
        CurMap.InitializeImmediately();
    }
    public void SelectTile(GridState state)
    {
        SelectedState = state;
    }

    public void SetTile(Vector3 pos)
    {
        Point p =  CurMap.GetPointViaPosition(pos);
        Debug.Log(p);
        CurMapData.BlocksX[p.X].BlocksY[p.Y] = (int)SelectedState;
        CurMap.RefreshMap();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(CurMapData);
#endif
    }

    private void Update()
    {
        CheckMouseInWindow();

        if (isMouseInsideWindow)
        {
            MoveCamera();
        }

        ZoomCamera();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SetTile(mousepos);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefreshMap();
        }
    }

    #region Camera_Handling
    public float scrollSpeed = 10f; // Speed at which the camera moves
    public float edgeSize = 10f;    // Distance from the edge to start scrolling
    public float zoomSpeed = 2f;    // Speed at which the camera zooms

    private Camera mainCamera;
    private bool isMouseInsideWindow = true;

    void Start()
    {
        mainCamera = Camera.main;
    }



    void CheckMouseInWindow()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
            Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
        {
            isMouseInsideWindow = false;
        }
        else
        {
            isMouseInsideWindow = true;
        }
    }

    void MoveCamera()
    {
        Vector3 cameraPosition = mainCamera.transform.position;

        if (Input.mousePosition.x <= edgeSize)
        {
            // Move camera left
            cameraPosition.x -= scrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.x >= Screen.width - edgeSize)
        {
            // Move camera right
            cameraPosition.x += scrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y <= edgeSize)
        {
            // Move camera down
            cameraPosition.y -= scrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y >= Screen.height - edgeSize)
        {
            // Move camera up
            cameraPosition.y += scrollSpeed * Time.deltaTime;
        }

        mainCamera.transform.position = cameraPosition;
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            mainCamera.orthographicSize -= scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, 2f, 20f); // Adjust these values as needed
        }
    }
    #endregion
}
