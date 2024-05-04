using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public IntVector2 size;

    public GameObject EndZonePrefab;

    public MazeCell cellPrefab;

    public MazeCelling mazeCelling;

    private MazeCell[,] cells;

    //float genrationStepDelay = 0.001f;

    public MazePassage passagePrefab;

    public MazeWall wallPrefab;

    public MazeWallWithDoor doorPrefab;

    public Material LightOffMaterial;

    public Material LightOnMaterial;

    public Camera birdEyeViewCamera;

    private Camera FPSViewCamera;

    private bool isDoorSet = false;

    public void GenerateMaze() {
        // WaitForSeconds delay = new WaitForSeconds(genrationStepDelay);
        cells = new MazeCell[size.x, size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0) {
            // yield return delay;
            DoNextGenerationStep(activeCells);
        }

        SetEndZone();
    }

    private MazeCell CreateCell (IntVector2 coordinates) {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition =
            new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        newCell.mazeCelling = CreateMazeCelling(coordinates);
        // CreateMazeCelling(coordinates);
        return newCell;
    }

    public MazeCelling CreateMazeCelling(IntVector2 coordinates) {
        MazeCelling newMazeCelling = Instantiate(mazeCelling) as MazeCelling;
        newMazeCelling.name = "Maze Celling " + coordinates.x + ", " + coordinates.z ;
        newMazeCelling.coordinates = coordinates;
        newMazeCelling.transform.parent = transform;
        newMazeCelling.transform.localPosition =
            new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);

        //1/10 chance to turn the point light off
        if (Random.Range(0, 15) == 0) {
            //Get the Point Light in the prefab
            Light pointLight = newMazeCelling.GetComponentInChildren<Light>();
            //Get the cube in the prefab
            GameObject cube = newMazeCelling.transform.GetChild(2).gameObject;


            //Turn the light off
            pointLight.enabled = false;
            MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
            cubeRenderer.material = LightOffMaterial;


            // if(pointLight != null)
            // pointLight.gameObject.GetComponent<LightFlicker>().enabled = true;
            // pointLight.AddComponent<LightFlicker>();
        }
        return newMazeCelling;
    }

    public void NightLight()
    {
        foreach (MazeCell cell in cells)
        {
            Light pointLight = cell.mazeCelling.GetComponentInChildren<Light>();
            if (pointLight != null)
            {
                pointLight.intensity = 0.85f;
                if (pointLight.enabled)
                {
                    GameObject cube = cell.mazeCelling.transform.GetChild(2).gameObject;
                    MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
                    cubeRenderer.material = LightOffMaterial;
                    pointLight.gameObject.GetComponent<LightFlicker>().enabled = true;
                }
            }

            foreach (MazeCellEdge edge in cell.edges)
            {
                if (edge is MazeWall)
                {
                    MeshRenderer meshRenderer = edge.transform.GetChild(0).GetComponentInChildren<MeshRenderer>();
                    meshRenderer.material.SetFloat("_IsDay", 0.0f);
                }
            }
        }
    }

    public void DayLight()
    {
        foreach (MazeCell cell in cells)
        {
            Light pointLight = cell.mazeCelling.GetComponentInChildren<Light>();
            if (pointLight != null)
            {
                pointLight.intensity = 5f;
                if (pointLight.enabled)
                {
                    GameObject cube = cell.mazeCelling.transform.GetChild(2).gameObject;
                    MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
                    cubeRenderer.material = LightOffMaterial;
                    pointLight.gameObject.GetComponent<LightFlicker>().enabled = false;
                }

            }

            foreach (MazeCellEdge edge in cell.edges)
            {
                if (edge is MazeWall)
                {
                    MeshRenderer meshRenderer = edge.transform.GetChild(0).GetComponentInChildren<MeshRenderer>();
                    meshRenderer.material.SetFloat("_IsDay", 1.0f);
                }
            }

        }
    }

    private void DoFirstGenerationStep(List<MazeCell> activeCells) {
        activeCells.Add(CreateCell(RandomCoordinates));
    }

    private void DoNextGenerationStep (List<MazeCell> activeCells) {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized) {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates)) {
            MazeCell neighbor = GetCell(coordinates);
            if (neighbor == null) {
                neighbor = CreateCell(coordinates);
                CreatePassage(currentCell, neighbor, direction);
                activeCells.Add(neighbor);
            }
            else {


                CreateWall(currentCell, neighbor, direction);

                // No longer remove the cell here.
            }
        }
        else {

            if (!isDoorSet)
            {
                CreateDoor(currentCell, null, direction);
                isDoorSet = true;
            }
            else
                CreateWall(currentCell, null, direction);
            // No longer remove the cell here.
        }
    }

    public IntVector2 RandomCoordinates {
        get {
            return new IntVector2(Random.Range(0, size.x - 1), Random.Range(0, size.z - 1));
        }
    }

    public MazeCell GetCell (IntVector2 coordinates) {
        return cells[coordinates.x, coordinates.z];
    }

    public bool ContainsCoordinates(IntVector2 coordinate) {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }

    private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);

        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {

        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        MazeDirection mazeDirection = direction;

        wall.Initialize(cell, otherCell, mazeDirection);
        SetWallMaterialDirection(wall, mazeDirection);

        if (otherCell != null) {
            wall = Instantiate(wallPrefab) as MazeWall;
            mazeDirection = direction.GetOpposite();
            wall.Initialize(otherCell, cell, mazeDirection);
            SetWallMaterialDirection(wall, mazeDirection);
        }
    }

    private void CreateDoor(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        Debug.Log("Creating Door at " + cell.coordinates.x + ", " + cell.coordinates.z);
        MazeWallWithDoor door = Instantiate(doorPrefab) as MazeWallWithDoor;
        MazeDirection mazeDirection = direction;

        door.Initialize(cell, otherCell, mazeDirection);

        if (otherCell != null)
        {
            MazeWall wall = Instantiate(wallPrefab) as MazeWall;
            mazeDirection = direction.GetOpposite();
            wall.Initialize(otherCell, cell, mazeDirection);
            SetWallMaterialDirection(wall, mazeDirection);
        }
    }

    public static void SetWallMaterialDirection(MazeCellEdge mazeCellEdge, MazeDirection mazeDirection)
    {
        int MaterialIndex = mazeCellEdge.MazeDirectionToNESWMaterialIndex(mazeDirection);
        Material materialToAssign = mazeCellEdge.NESWMaterials[MaterialIndex];

        MeshRenderer[] meshRenderers = mazeCellEdge.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.material = materialToAssign;
        }
    }

    public void SetEndZone()
    {
        MazeCell lastCell = GetCell(new IntVector2(size.x - 1,size.z - 1));
        lastCell.mazeCelling.gameObject.GetComponentInChildren<Light>().enabled = true;
        lastCell.mazeCelling.gameObject.GetComponentInChildren<Light>().color = Color.green;
        GameObject endZoneInstance = Instantiate(EndZonePrefab, lastCell.transform);
    }

    public MazeCell[,] GetCells()
    {
        return cells;
    }
}
