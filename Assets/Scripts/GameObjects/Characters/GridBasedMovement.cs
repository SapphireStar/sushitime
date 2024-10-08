using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasedMovement : MonoBehaviour
{
    [HideInInspector]
    public float HorizontalSpeed = 5.0f;
    [HideInInspector]
    public float VerticalSpeed = 3.5f;
    #region Path_Finding
    [SerializeField]
    protected GridMap m_gridMap;

    protected Point m_curPoint;
    protected Vector3 m_curPos;

    public Point CurPoint
    {
        get => m_curPoint;
    }
    public Vector3 CurPos
    {
        get => m_curPos;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        StopAllCoroutines();
        m_curPos = transform.position;
        m_curPoint = m_gridMap.GetPointViaPosition(m_curPos);
    }

    // Update is called once per frame
    void Update()
    {
    }
    #region Path_Finding
    public void SetGridMap(GridMap map)
    {
        m_gridMap = map;
    }
    public virtual IEnumerator MoveTo(Point targetPoint)
    {
        m_curPoint = m_gridMap.GetPointViaPosition(m_curPos);

        var stack = StartPathFinding(m_curPoint, targetPoint);
        yield return StartCoroutine(RapidMove(stack));
    }
    public virtual IEnumerator MoveTo(Vector3 targetPos)
    {
        m_curPoint = m_gridMap.GetPointViaPosition(m_curPos);
        //transform.position = m_gridMap.GetPositionViaPoint(m_curPoint);
        var stack = StartPathFinding(m_curPoint, m_gridMap.GetPointViaPosition(targetPos));
        yield return StartCoroutine(RapidMove(stack));

    }
    public virtual IEnumerator MoveTo(Point targetPoint, int remainSteps)
    {
        m_curPoint = m_gridMap.GetPointViaPosition(m_curPos);

        var stack = StartPathFinding(m_curPoint, targetPoint, remainSteps);
        yield return StartCoroutine(RapidMove(stack));
    }
    public virtual IEnumerator MoveTo(Vector3 targetPos, int remainSteps)
    {
        m_curPoint = m_gridMap.GetPointViaPosition(m_curPos);
        //transform.position = m_gridMap.GetPositionViaPoint(m_curPoint);
        var stack = StartPathFinding(m_curPoint, m_gridMap.GetPointViaPosition(targetPos), remainSteps);
        yield return StartCoroutine(RapidMove(stack));

    }
/*    protected virtual IEnumerator RapidMove(Stack<Point> pathStack)
    {
        while (pathStack.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            Point nextTarget = pathStack.Pop();
            transform.position = m_gridMap.GetPositionViaPoint(nextTarget);
            m_curPoint = m_gridMap.GetPointViaPosition(transform.position);
            m_curPos = transform.position;
        }

    }*/
    protected IEnumerator RapidMove(Stack<Point> pathStack)
    {
        while (pathStack.Count > 0)
        {
            Point nextTarget = pathStack.Pop();
            Vector3 targetPos = m_gridMap.GetPositionViaPoint(nextTarget);

            while (Vector3.Distance(targetPos, transform.position) > 0.1f)
            {
                Vector3 moveDir = Vector3.Normalize(targetPos - transform.position);

                if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
                {
                    transform.Translate(moveDir * Time.deltaTime * HorizontalSpeed);
                }
                else
                {
                    transform.Translate(moveDir * Time.deltaTime * VerticalSpeed);
                }

                if (moveDir.x > 0 && transform.localScale.x < 0)
                {
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                }
                else if (moveDir.x < 0 && transform.localScale.x > 0)
                {
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                }
                yield return new WaitForEndOfFrame();
            }

            m_curPoint = m_gridMap.GetPointViaPosition(transform.position);
            m_curPos = transform.position;
        }
    }


    
    protected virtual bool CheckGridWalkAvailable(Point target, BFSTree cur)
    {
        bool res = true;
        if (target.X < 0 || target.Y < 0 || target.X > m_gridMap.StepWidth - 1 || target.Y > m_gridMap.StepHeight - 1)
        {
            res &= false;
        }

        if (m_gridMap.IsObstacle(m_gridMap.GetPointState(target)))
        {
            res &= false;
        }

        //Enemy can't walk to neighbour point that upon it or below it
        if(cur.Parent!=null)
        {
            Point prevPoint = cur.Parent.Value;
            GridState prevState = m_gridMap.GetPointState(prevPoint);
            GridState curState = m_gridMap.GetPointState(target);
            if (prevPoint.X == target.X && Mathf.Abs(prevPoint.Y - target.Y) == 1)
            {
                if (prevState == curState && (prevState & GridState.None)>0)
                {
                    res &= false;
                }
                else if (((prevState & GridState.None)>0 && (curState & GridState.Ladder)>0)
                    || ((prevState & GridState.Ladder)>0 && (curState & GridState.None)>0))
                {
                    res &= false;
                }
            }
        }

        return res;
    }

    HashSet<Point> traversedNodes;
    Queue<BFSTree> queue;
    public Stack<Point> StartPathFinding(Point Start, Point Target)
    {
        queue = new Queue<BFSTree>();
        traversedNodes = new HashSet<Point>();

        BFSTree StartNode = getBFSTree(null, Start);
        queue.Enqueue(StartNode);
        traversedNodes.Add(Start);

        var res = PathFinding(null, Target);
        CollectBFSTree(StartNode);
        return res;
    }
    public Stack<Point> StartPathFinding(Point Start, Point Target, int remainSteps)
    {
        queue = new Queue<BFSTree>();
        traversedNodes = new HashSet<Point>();

        BFSTree StartNode = getBFSTree(null, Start);
        queue.Enqueue(StartNode);
        traversedNodes.Add(Start);

        var res = PathFinding(null, Target, remainSteps);
        CollectBFSTree(StartNode);
        return res;
    }

    private Stack<Point> PathFinding(BFSTree StartNode, Point target)
    {
        while (queue.Count > 0)
        {
            BFSTree cur = queue.Dequeue();
            Point curPoint = cur.Value;

            if (!CheckGridWalkAvailable(cur.Value, cur))
            {
                continue;
            }

            if (curPoint.Equals(target))
            {
                return CalculatePath(cur);
            }

            Point up = new Point(curPoint.X, curPoint.Y + 1);
            Point left = new Point(curPoint.X - 1, curPoint.Y);
            Point down = new Point(curPoint.X, curPoint.Y - 1);
            Point right = new Point(curPoint.X + 1, curPoint.Y);

            if (!traversedNodes.Contains(up))
            {
                traversedNodes.Add(up);
                BFSTree upNode = getBFSTree(cur, up);
                cur.Children.Add(upNode);
                queue.Enqueue(upNode);
            }

            if (!traversedNodes.Contains(left))
            {
                traversedNodes.Add(left);
                BFSTree leftNode = getBFSTree(cur, left);
                cur.Children.Add(leftNode);
                queue.Enqueue(leftNode);
            }

            if (!traversedNodes.Contains(down))
            {
                traversedNodes.Add(down);
                BFSTree downNode = getBFSTree(cur, down);
                cur.Children.Add(downNode);
                queue.Enqueue(downNode);
            }

            if (!traversedNodes.Contains(right))
            {
                traversedNodes.Add(right);
                BFSTree rightNode = getBFSTree(cur, right);
                cur.Children.Add(rightNode);
                queue.Enqueue(rightNode);
            }
        }
        Debug.Log($"Pathfinding failed, can't find point{target}");
        return null;


    }
    private Stack<Point> PathFinding(BFSTree StartNode, Point target, int remainSteps)
    {
        while (queue.Count > 0)
        {
            BFSTree cur = queue.Dequeue();
            Point curPoint = cur.Value;

            if (!CheckGridWalkAvailable(cur.Value, cur))
            {
                continue;
            }

            if (curPoint.Equals(target))
            {
                return CalculatePath(cur, remainSteps);
            }

            Point up = new Point(curPoint.X, curPoint.Y + 1);
            Point left = new Point(curPoint.X - 1, curPoint.Y);
            Point down = new Point(curPoint.X, curPoint.Y - 1);
            Point right = new Point(curPoint.X + 1, curPoint.Y);

            if (!traversedNodes.Contains(up))
            {
                traversedNodes.Add(up);
                BFSTree upNode = getBFSTree(cur, up);
                cur.Children.Add(upNode);
                queue.Enqueue(upNode);
            }

            if (!traversedNodes.Contains(left))
            {
                traversedNodes.Add(left);
                BFSTree leftNode = getBFSTree(cur, left);
                cur.Children.Add(leftNode);
                queue.Enqueue(leftNode);
            }

            if (!traversedNodes.Contains(down))
            {
                traversedNodes.Add(down);
                BFSTree downNode = getBFSTree(cur, down);
                cur.Children.Add(downNode);
                queue.Enqueue(downNode);
            }

            if (!traversedNodes.Contains(right))
            {
                traversedNodes.Add(right);
                BFSTree rightNode = getBFSTree(cur, right);
                cur.Children.Add(rightNode);
                queue.Enqueue(rightNode);
            }
        }
        Debug.Log($"Pathfinding failed, can't find point{target}");
        return null;


    }
    private Stack<Point> CalculatePath(BFSTree lastNode)
    {
        Stack<Point> res = new Stack<Point>();
        res.Push(lastNode.Value);
        while (lastNode.Parent != null)
        {
            lastNode = lastNode.Parent;
            res.Push(lastNode.Value);
        }
        return res;
    }

    private Stack<Point> CalculatePath(BFSTree lastNode, int remainSteps)
    {
        Stack<Point> res = new Stack<Point>();
        int totalLength = 0;
        BFSTree curnode = lastNode;
        //Calculate the route length to the target point, if longer than remainSteps, then remove the 
        //exceeded nodes
        while (curnode != null)
        {
            totalLength++;
            curnode = curnode.Parent;
        }
        if (remainSteps < totalLength - 1)
        {
            for (int i = 0; i < totalLength - 1 - remainSteps; i++)
            {
                lastNode = lastNode.Parent;
            }
        }

        res.Push(lastNode.Value);
        while (lastNode.Parent != null)
        {
            lastNode = lastNode.Parent;
            res.Push(lastNode.Value);
        }
        return res;
    }

    private Queue<BFSTree> objectPool;
    private BFSTree getBFSTree(BFSTree parent, Point value)
    {
        if (objectPool == null)
        {
            objectPool = new Queue<BFSTree>();
        }
        if (objectPool.Count > 0)
        {
            BFSTree res = objectPool.Dequeue();
            res.Parent = parent;
            res.Value = value;
            return res;
        }
        else
        {
            BFSTree res = new BFSTree(parent, value);
            return res;
        }
    }
    private void CollectBFSNode(BFSTree node)
    {
        node.Reset();
        objectPool.Enqueue(node);
    }
    private void CollectBFSTree(BFSTree head)
    {
        foreach (var item in head.Children)
        {
            CollectBFSTree(item);
        }
        CollectBFSNode(head);
    }
}

public class BFSTree
{
    public Point Value;
    public BFSTree Parent;
    public List<BFSTree> Children;

    public BFSTree()
    {
        Children = new List<BFSTree>();
    }
    public void Reset()
    {
        Value = new Point(-1, -1);
        Parent = null;
        Children = new List<BFSTree>();
    }
    public BFSTree(BFSTree father)
    {
        Parent = father;
        Children = new List<BFSTree>();
    }
    public BFSTree(Point value)
    {
        Value = value;
        Children = new List<BFSTree>();
    }
    public BFSTree(BFSTree father, Point value)
    {
        Value = value;
        Parent = father;
        Children = new List<BFSTree>();
    }
    public BFSTree(BFSTree father, BFSTree[] children)
    {
        Parent = father;
        Children = new List<BFSTree>();
    }
#endregion

}
