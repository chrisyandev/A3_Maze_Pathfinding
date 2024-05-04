using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathMarker
{
    public MazeCell cell;
    public float G;
    public float H;
    public float F;
    public GameObject markObj;
    public PathMarker parentNode;

    public PathMarker(MazeCell cell, float g, float h, float f, GameObject markObj, PathMarker parentNode)
    {
        this.cell = cell;
        G = g;
        H = h;
        F = f;
        this.markObj = markObj;
        this.parentNode = parentNode;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return cell.coordinates.Equals(((PathMarker)obj).cell.coordinates);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class Pathfinder : MonoBehaviour
{
    public GameManager manager;
    public Material closedMat;
    public Material openMat;
    public GameObject startMarkObj;
    public GameObject goalMarkObj;
    public GameObject midMarkObj;

    private Maze maze;
    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastFoundNode;
    bool done = false;
    bool hasStarted = false;

    List<PathMarker> openNodes = new List<PathMarker>();
    List<PathMarker> closedNodes = new List<PathMarker>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    { 
        if (!manager.finishedGeneratingMaze) return;

        if (maze == null)
        {
            maze = manager.mazeInstance;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            BeginSearch();
            hasStarted = true;
        }

        if (hasStarted)
        {
            if (Input.GetKeyDown(KeyCode.J) && !done)
            {
                Search(lastFoundNode);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            GetPath();
        }
    }

    void Search(PathMarker theNode)
    {
        if (theNode == null) return;
        if (theNode.Equals(goalNode))
        {
            done = true;
            return;
        }

        foreach (MazeDirection dir in (MazeDirection[]) Enum.GetValues(typeof(MazeDirection)))
        {
            IntVector2 neighbour = MazeDirections.ToIntVector2(dir) + theNode.cell.coordinates;

            if (theNode.cell.GetEdge(dir) is MazeWall) continue;
            if (neighbour.x < 0 || neighbour.x > maze.size.x - 1 || neighbour.z < 0 || neighbour.z > maze.size.z - 1) continue;
            if (IsClosed(maze.GetCell(neighbour))) continue;

            float g = Vector2.Distance(theNode.cell.transform.position, maze.GetCell(neighbour).transform.position) + theNode.G;
            float h = Vector2.Distance(maze.GetCell(neighbour).transform.position, goalNode.cell.transform.position);
            float f = g + h;

            GameObject pathBlock = Instantiate(midMarkObj, maze.GetCell(neighbour).transform.position, Quaternion.identity);

            TextMeshProUGUI[] values = pathBlock.GetComponentsInChildren<TextMeshProUGUI>();

            values[0].text = "FrStart: " + g.ToString("0.00");
            values[1].text = "ToGoal: " + h.ToString("0.00");
            values[2].text = "Total: " + f.ToString("0.00");

            if (!UpdateMarker(maze.GetCell(neighbour), g, h, f, theNode))
            {
                openNodes.Add(new PathMarker(maze.GetCell(neighbour), g, h, f, pathBlock, theNode));
            }
        }

        openNodes = openNodes.OrderBy(p => p.F).ToList();

        PathMarker newClosedNode = openNodes.ElementAt(0);
        newClosedNode.markObj.GetComponent<Renderer>().material = closedMat;
        closedNodes.Add(newClosedNode);
        openNodes.RemoveAt(0);

        lastFoundNode = newClosedNode;
    }

    bool UpdateMarker(MazeCell cellToUpdate, float g, float h, float f, PathMarker prt)
    {

        foreach (PathMarker p in openNodes)
        {

            if (p.cell.coordinates.Equals(cellToUpdate.coordinates))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parentNode = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MazeCell cellToCheck)
    {
        foreach (PathMarker p in closedNodes)
        {
            if (p.cell.coordinates.Equals(cellToCheck.coordinates)) return true;
        }
        return false;
    }

    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker currentNode = lastFoundNode;
        PathMarker lastInstantiatedNode = null;

        while (!startNode.Equals(currentNode) && currentNode != null)
        {
            GameObject markObj = Instantiate(midMarkObj, currentNode.cell.transform.position, Quaternion.identity);

            if (lastInstantiatedNode != null)
            {
                markObj.transform.LookAt(lastInstantiatedNode.cell.transform.position);                                
                FindRecursive(markObj.transform, "Arrow").gameObject.SetActive(true);
            }

            lastInstantiatedNode = currentNode;
            currentNode = currentNode.parentNode;
        }

        Instantiate(midMarkObj, startNode.cell.transform.position, Quaternion.identity);
    }

    Quaternion MultiplyQuaternions(Quaternion q1, Quaternion q2)
    {
        float w = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
        float x = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
        float y = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
        float z = q1.w * q2.z + q1.x * q2.y - q1.y * q2.y + q1.z * q2.w;
        return new Quaternion(x, y, z, w);
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        MazeCell startCell = maze.GetCell(new IntVector2(0, 0));
        startNode = new PathMarker(startCell, 0.0f, 0.0f, 0.0f, Instantiate(startMarkObj, startCell.transform.position, Quaternion.identity), null);

        MazeCell goalCell = maze.GetCell(new IntVector2(maze.size.x - 1, maze.size.z - 1));
        goalNode = new PathMarker(goalCell, 0.0f, 0.0f, 0.0f, Instantiate(goalMarkObj, goalCell.transform.position, Quaternion.identity), null); ;

        openNodes.Clear();
        closedNodes.Clear();

        openNodes.Add(startNode);
        lastFoundNode = startNode;
    }

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Mark");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    GameObject FindRecursive(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent.gameObject;
        }
        foreach (Transform child in parent)
        {
            GameObject result = FindRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
