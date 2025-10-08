using System.Collections.Generic;
using UnityEngine;

public enum MazeMode
{
    Goal,
    Exit
}

public class MazeGenerator : MonoBehaviour
{
    [Header("Configuração do Labirinto")]
    public int rows = 10;
    public int cols = 10;
    public float cellWidth = 2f;
    public float cellLength = 2f;
    public float wallHeight = 2f;

    public MazeMode mode = MazeMode.Goal;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject wallPrefab;

    private Cell[,] grid;

    // Posicionamento do player e célula alvo
    public Vector2Int playerStart;
    public Vector2Int targetCell; // usado no Goal ou Exit

    void Start()
    {
        GenerateMaze();
    }

    #region Geração do Labirinto
    public void GenerateMaze()
    {
        grid = new Cell[rows, cols];

        // Instancia células
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * cellWidth, 0, z * cellLength);
                GameObject obj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);

                Cell cell = obj.GetComponent<Cell>();
                cell.width = cellWidth;
                cell.length = cellLength;
                cell.wallHeight = wallHeight;

                cell.UpdateFloor();
                cell.UpdateBoxCollider();

                cell.CreateWalls(wallPrefab);

                grid[z, x] = cell;
            }

            // Remove aberturas
            RemoveWallForEdge(playerStart);
            if (mode == MazeMode.Exit)
            {
                RemoveWallForEdge(targetCell);
            }
        }

        // Escolhe entrada(s)
        if (mode == MazeMode.Goal)
        {
            playerStart = new Vector2Int(0, 0); // canto superior esquerdo
        }
        else if (mode == MazeMode.Exit)
        {
            playerStart = new Vector2Int(0, 0); // entrada
            targetCell = new Vector2Int(rows - 1, cols - 1); // saída
        }

        // Remove abertura inicial
        RemoveWallForEdge(playerStart);

        // Modo Exit: remove abertura de saída
        if (mode == MazeMode.Exit)
        {
            RemoveWallForEdge(targetCell);
        }

        // Gera labirinto via DFS
        GenerateMazeDFS(playerStart.x, playerStart.y, new bool[rows, cols]);

        // Para modo Goal, seleciona célula alvo mais distante
        if (mode == MazeMode.Goal)
        {
            targetCell = FindFurthestCell(playerStart);
        }
    }
    #endregion

    #region DFS Recursivo
    private void GenerateMazeDFS(int row, int col, bool[,] visited)
    {
        visited[row, col] = true;

        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(row - 1, col),
            new Vector2Int(row + 1, col),
            new Vector2Int(row, col - 1),
            new Vector2Int(row, col + 1)
        };

        // Embaralha os vizinhos
        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int temp = neighbors[i];
            int rand = Random.Range(0, neighbors.Count);
            neighbors[i] = neighbors[rand];
            neighbors[rand] = temp;
        }

        foreach (var n in neighbors)
        {
            if (n.x >= 0 && n.x < rows && n.y >= 0 && n.y < cols && !visited[n.x, n.y])
            {
                // Remove parede entre (row,col) e n
                RemoveWallBetween(grid[row, col], grid[n.x, n.y]);
                GenerateMazeDFS(n.x, n.y, visited);
            }
        }
    }
    #endregion

    #region Remoção de paredes
    private void RemoveWallBetween(Cell a, Cell b)
    {
        Vector2Int diff = new Vector2Int(
            Mathf.RoundToInt(b.transform.localPosition.x - a.transform.localPosition.x),
            Mathf.RoundToInt(b.transform.localPosition.z - a.transform.localPosition.z)
        );

        if (diff.x > 0) { Destroy(a.wallEast); Destroy(b.wallWest); } // b está à direita
        else if (diff.x < 0) { Destroy(a.wallWest); Destroy(b.wallEast); } // b está à esquerda
        else if (diff.y > 0) { Destroy(a.wallNorth); Destroy(b.wallSouth); } // b está acima
        else if (diff.y < 0) { Destroy(a.wallSouth); Destroy(b.wallNorth); } // b está abaixo
    }

    private void RemoveWallForEntry(Vector2Int pos)
    {
        Cell c = grid[pos.x, pos.y];
        if (pos.x == 0) Destroy(c.wallWest);
        else if (pos.x == rows - 1) Destroy(c.wallEast);
        else if (pos.y == 0) Destroy(c.wallSouth);
        else if (pos.y == cols - 1) Destroy(c.wallNorth);
    }

    private void RemoveWallForEdge(Vector2Int pos)
    {
        Cell c = grid[pos.x, pos.y];

        if (pos.x == 0) Destroy(c.wallWest);
        else if (pos.x == rows - 1) Destroy(c.wallEast);
        else if (pos.y == 0) Destroy(c.wallSouth);
        else if (pos.y == cols - 1) Destroy(c.wallNorth);
    }

    #endregion

    #region Encontrar célula mais distante
    private Vector2Int FindFurthestCell(Vector2Int start)
    {
        // BFS simples para encontrar célula mais distante
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[rows, cols];
        Vector2Int furthest = start;

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            furthest = curr;

            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                new Vector2Int(curr.x - 1, curr.y),
                new Vector2Int(curr.x + 1, curr.y),
                new Vector2Int(curr.x, curr.y - 1),
                new Vector2Int(curr.x, curr.y + 1)
            };

            foreach (var n in neighbors)
            {
                if (n.x >= 0 && n.x < rows && n.y >= 0 && n.y < cols && !visited[n.x, n.y])
                {
                    // Só considera vizinhos com parede removida
                    if (HasNoWallBetween(grid[curr.x, curr.y], grid[n.x, n.y]))
                    {
                        visited[n.x, n.y] = true;
                        queue.Enqueue(n);
                    }
                }
            }
        }

        return furthest;
    }

    private bool HasNoWallBetween(Cell a, Cell b)
    {
        Vector2Int diff = new Vector2Int(
            Mathf.RoundToInt(b.transform.localPosition.x - a.transform.localPosition.x),
            Mathf.RoundToInt(b.transform.localPosition.z - a.transform.localPosition.z)
        );

        if (diff.x > 0) return a.wallEast == null;
        if (diff.x < 0) return a.wallWest == null;
        if (diff.y > 0) return a.wallNorth == null;
        if (diff.y < 0) return a.wallSouth == null;

        return false;
    }
    #endregion
}
