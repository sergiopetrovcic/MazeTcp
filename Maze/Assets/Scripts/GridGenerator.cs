using UnityEngine;

[ExecuteInEditMode] // permite ver no Editor sem dar Play
public class GridGenerator : MonoBehaviour
{
    [Header("Configurações do Grid")]
    public int rows = 5;
    public int cols = 5;
    public float cellWidth = 2f;
    public float cellLength = 2f;
    public float wallHeight = 2f;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject wallPrefab;

    private Cell[,] grid;

    void Start()
    {
        // Só gera na execução
        if (Application.isPlaying)
            GenerateGrid();
    }

    public void GenerateGrid()
    {
        grid = new Cell[rows, cols];

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

                cell.CreateWalls(wallPrefab);

                grid[z, x] = cell;
            }
        }
    }

    // Gizmos — desenha grid visível no editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int z = 0; z <= rows; z++)
        {
            Vector3 start = transform.position + new Vector3(0, 0, z * cellLength);
            Vector3 end = transform.position + new Vector3(cols * cellWidth, 0, z * cellLength);
            Gizmos.DrawLine(start, end);
        }

        for (int x = 0; x <= cols; x++)
        {
            Vector3 start = transform.position + new Vector3(x * cellWidth, 0, 0);
            Vector3 end = transform.position + new Vector3(x * cellWidth, 0, rows * cellLength);
            Gizmos.DrawLine(start, end);
        }

        // Opcional: desenhar altura das paredes como guia visual
        Gizmos.color = Color.yellow;
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 basePos = transform.position + new Vector3(x * cellWidth + cellWidth / 2f, 0, z * cellLength + cellLength / 2f);
                Gizmos.DrawWireCube(basePos + Vector3.up * wallHeight / 2f, new Vector3(cellWidth, wallHeight, cellLength));
            }
        }
    }
}
