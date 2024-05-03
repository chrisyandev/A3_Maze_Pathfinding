using UnityEngine;

public abstract class MazeCellEdge : MonoBehaviour
{
    public Material[] NESWMaterials;
    public MazeCell cell, otherCell;
	
	public MazeDirection direction;

    public void Initialize (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		this.cell = cell;
		this.otherCell = otherCell;
		this.direction = direction;
		cell.SetEdge(direction, this);
		transform.parent = cell.transform;
		transform.localPosition = Vector3.zero;
        transform.localRotation = direction.ToRotation();
	}

    public int MazeDirectionToNESWMaterialIndex(MazeDirection mazeDirection)
    {
        switch(mazeDirection)
        {
            case MazeDirection.North:
                return 0;
            case MazeDirection.South:
                return 1;
            case MazeDirection.East:
                return 2;
            case MazeDirection.West:
                return 3;

            default:
                return 0;
        }
    }
}
