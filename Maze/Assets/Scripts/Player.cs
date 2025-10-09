using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public float visionRadius = 10f;
    public Cell currentCell;
    private Vector3 targetPosition;

    void Start()
    {
        if (maze == null)
            maze = FindFirstObjectByType<MazeGenerator>();

        // Define célula inicial
        currentCell = maze.GetCell(maze.playerStart);
        transform.position = currentCell.transform.position + Vector3.up * 1f; // acima do chão
        targetPosition = transform.position;
    }

    void Update()
    {
        if (maze != null)
            maze.RevealFog(transform.position, visionRadius);
    }

    #region Inputs
    public void RotateCCW()
    {
        //RotateSmooth(-90f);
        transform.Rotate(0f, -90f, 0f, Space.World);
    }

    public void RotateCW()
    {
        //RotateSmooth(90f);
        transform.Rotate(0f, 90f, 0f, Space.World);
    }

    public void Restart()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        currentCell = maze.GetCell(maze.playerStart);
    }

    public void MoveToNextCell()
    {
        //Cell next = maze.GetNeighborGridAligned(currentCell);
        Cell next = maze.GetNeighbor(currentCell, transform.forward);
        //Cell next = maze.GetNeighborSingleStep(currentCell, transform.forward);
        if (next != null)
        {
            currentCell = next;
            //targetPosition = new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z);
            //isMoving = true;
            transform.position = new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z);
        }
        else
        {
            Debug.Log("Parede bloqueando o caminho ou fora dos limites.");
        }
    }
    #endregion
}
