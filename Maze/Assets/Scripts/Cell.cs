using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Dimensões")]
    public float width = 1f;
    public float length = 1f;
    public float wallHeight = 2f;

    [Header("Paredes")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("Status da Célula")]
    public bool discovered = false;
    public int visitCount = 0;

    [Header("Chão")]
    public GameObject floor; // arraste o quad/plane aqui

    private void Start()
    {
        // Garante que o chão tenha um trigger
        if (floor != null)
        {
            Collider c = floor.GetComponent<Collider>();
            if (c != null)
                c.isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrou");
        FindFirstObjectByType<MazeManager>()?.CellEntered(this);
        if (other.CompareTag("Player"))
        {
            if (!discovered)
            {
                discovered = true;
                ChangeColor(Color.red);
                Debug.Log($"Célula descoberta pela primeira vez: {name}");
            }

            visitCount++;
            Debug.Log($"Player entrou na célula {name}, total de visitas: {visitCount}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Saiu");
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player saiu da célula {name}");
        }
    }

    public void UpdateFloor()
    {
        if (floor != null)
            floor.transform.localScale = new Vector3(width, length, 1f);
    }


    public void UpdateBoxCollider()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(width, 0.1f, length); // tamanho base relativo à escala
            collider.center = new Vector3(0f, 0f, 0f); // centralizado
        }
    }

    // Cria as paredes e posiciona elas corretamente
    public void CreateWalls(GameObject wallPrefab)
    {
        float halfW = width / 2f;
        float halfL = length / 2f;

        // Norte
        wallNorth = Instantiate(wallPrefab, transform);
        wallNorth.transform.localPosition = new Vector3(0, wallHeight / 2f, halfL);
        wallNorth.transform.localScale = new Vector3(width, wallHeight, 0.1f);

        // Sul
        wallSouth = Instantiate(wallPrefab, transform);
        wallSouth.transform.localPosition = new Vector3(0, wallHeight / 2f, -halfL);
        wallSouth.transform.localScale = new Vector3(width, wallHeight, 0.1f);

        // Leste
        wallEast = Instantiate(wallPrefab, transform);
        wallEast.transform.localPosition = new Vector3(halfW, wallHeight / 2f, 0);
        wallEast.transform.localScale = new Vector3(0.1f, wallHeight, length);

        // Oeste
        wallWest = Instantiate(wallPrefab, transform);
        wallWest.transform.localPosition = new Vector3(-halfW, wallHeight / 2f, 0);
        wallWest.transform.localScale = new Vector3(0.1f, wallHeight, length);
    }

    // Marca como descoberta
    public void Descobrir()
    {
        discovered = true;
        visitCount++;
    }

    public void ChangeColor(Color color)
    {
        if (floor != null)
        {
            MeshRenderer quadRenderer = floor.GetComponent<MeshRenderer>();
            if (quadRenderer != null && quadRenderer.material != null)
            {
                quadRenderer.material.color = color;
            }
            else
            {
                Debug.LogWarning($"Floor de {name} não tem MeshRenderer ou material.");
            }
        }
        else
        {
            Debug.LogWarning($"Floor não atribuído em {name}.");
        }
    }
}
