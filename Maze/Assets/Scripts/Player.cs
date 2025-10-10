using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public MazeGenerator maze;
    public Cell cell;
    public float visionRadius = 10f;
    public Cell currentCell;
    private Vector3 targetPosition;

    // Statistics
    [HideInInspector] public int epochs = 0;

    [HideInInspector] public int rotations = 0;
    [HideInInspector] public int leftRotations = 0;
    [HideInInspector] public int rightRotations = 0;
    [HideInInspector] public int steps = 0;
    [HideInInspector] public int moves = 0;
    [HideInInspector] public float timer = 0;
    [HideInInspector] public int cellsDiscovered = 0;
    [HideInInspector] public int cellsVisited = 0;

    [HideInInspector] public int rotationsCurrentEpoch = 0;
    [HideInInspector] public int leftRotationsCurrentEpoch = 0;
    [HideInInspector] public int rightRotationsCurrentEpoch = 0;
    [HideInInspector] public int stepsCurrentEpoch = 0;
    [HideInInspector] public int movesCurrentEpoch = 0;
    [HideInInspector] public float timerCurrentEpoch = 0;
    [HideInInspector] public int cellsDiscoveredCurrentEpoch = 0;
    [HideInInspector] public int cellsVisitedCurrentEpoch = 0;

    // Eventos
    public delegate void PlayerEvent(params object[] args);
    public event PlayerEvent OnStep;
    public event PlayerEvent OnRotateLeft;
    public event PlayerEvent OnRotateRight;
    public event PlayerEvent OnRotate;
    public event PlayerEvent OnMove;

    private void Awake()
    {
        maze.OnCellDiscovered += Maze_OnCellDiscovered;
        maze.OnCellVisited += Maze_OnCellVisited;
        maze.OnFinish += Maze_OnFinish;
    }

    private void Maze_OnCellVisited(params object[] args)
    {
        cellsVisitedCurrentEpoch++;
        cellsVisited++;
    }

    private void Maze_OnCellDiscovered(params object[] args)
    {
        cellsDiscoveredCurrentEpoch++;
        cellsDiscovered++;
    }

    private void Maze_OnFinish(params object[] args)
    {
        Debug.Log(Time.time.ToString("F3") + " - Época " + epochs + " encerrada em " + args[0] + " segundos com " + args[2] + " células visitadas! Total de células visitadas é " + args[1] + ".");
        Restart();
    }

    private void OnDestroy()
    {
        maze.OnFinish -= Maze_OnFinish;
        maze.OnCellDiscovered -= Maze_OnCellDiscovered;
        maze.OnCellVisited -= Maze_OnCellVisited;
    }

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
        transform.Rotate(0f, -90f, 0f, Space.World);
        leftRotationsCurrentEpoch++;
        rotationsCurrentEpoch++;
        movesCurrentEpoch++;
        leftRotations++;
        rotations++;
        moves++;
        try { OnRotateLeft?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotateLeft error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCCW() > OnMove error: " + e); }
    }

    public void RotateCW()
    {
        transform.Rotate(0f, 90f, 0f, Space.World);
        rightRotationsCurrentEpoch++;
        rotationsCurrentEpoch++;
        movesCurrentEpoch++;
        rightRotations++;
        rotations++;
        moves++;
        try { OnRotateRight?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotateRight error: " + e); }
        try { OnRotate?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnRotate error: " + e); }
        try { OnMove?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnMove error: " + e); }
    }

    public void Restart()
    {
        epochs++;
        rotationsCurrentEpoch = 0;
        leftRotationsCurrentEpoch = 0;
        rightRotationsCurrentEpoch = 0;
        stepsCurrentEpoch = 0;
        movesCurrentEpoch = 0;
        timerCurrentEpoch = 0;
        cellsDiscoveredCurrentEpoch = 0;
        cellsVisitedCurrentEpoch = 0;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        currentCell = maze.GetCell(maze.playerStart);
        maze.NewEpoch();
    }

    public void MoveToNextCell()
    {
        Cell next = maze.GetNeighbor(currentCell, transform.forward);
        if (next != null)
        {
            currentCell = next;
            stepsCurrentEpoch++;
            movesCurrentEpoch++;
            steps++;
            moves++;
            try { OnStep?.Invoke(); }
            catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > MoveToNextCell() > OnStep error: " + e); }
            try { OnMove?.Invoke(); }
            catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - Player > RotateCW() > OnMove error: " + e); }
            // Manobra necessária para garantir a ordem dos eventos:
            StartCoroutine(MoveAndTriggerSequence(next, new Vector3(next.transform.position.x, transform.position.y, next.transform.position.z)));
        }
        else
            Debug.Log("Parede bloqueando o caminho ou fora dos limites.");
    }
    #endregion

    private IEnumerator MoveAndTriggerSequence(Cell next, Vector3 novaPosicao)
    {
        transform.position = novaPosicao;

        // Isso força a Unity a processar o OnTriggerEnter (e2) AGORA.
        yield return new WaitForFixedUpdate();

        if (next.Coordinates == maze.targetCell)
            maze.Finish();
    }
}
