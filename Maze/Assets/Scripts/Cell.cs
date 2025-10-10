using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Dimensões")]
    public float width = 1f;
    public float length = 1f;
    public float wallHeight = 2f;
    [HideInInspector]
    public Vector2Int Coordinates;

    [Header("Paredes")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("Chão")]
    public GameObject floor; // arraste o quad/plane aqui

    // Define um limite máximo para normalização
    float maxVisits = 10f; // ajuste conforme o esperado no seu jogo

    // Statistics
    private bool discovered = false;
    private int visitCount = 0;

    // Eventos
    public delegate void CellEvent(params object[] args);
    public event CellEvent OnCellVisit;
    public event CellEvent OnNewDiscover;
    
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
        FindFirstObjectByType<Manager>()?.CellEntered(this);
        if (other.CompareTag("Player"))
        {
            if (!discovered)
            {
                discovered = true;
                try { OnNewDiscover?.Invoke(); }
                catch (Exception e) { Debug.LogError("Cell > OnTriggerEnter() > OnNewDiscover error: " + e); }
            }
            visitCount++;
            try { OnCellVisit?.Invoke(); }
            catch (Exception e) { Debug.LogError("Cell > OnTriggerEnter() > OnCellVisit error: " + e); }
            ChangeColor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log($"Player saiu da célula {name}");
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

    public void ChangeColor()
    {
        if (floor == null)
        {
            Debug.LogWarning($"Floor não atribuído em {name}.");
            return;
        }

        MeshRenderer quadRenderer = floor.GetComponent<MeshRenderer>();
        if (quadRenderer == null || quadRenderer.material == null)
        {
            Debug.LogWarning($"Floor de {name} não tem MeshRenderer ou material.");
            return;
        }

        float t = Mathf.Clamp01(visitCount / maxVisits);

        // Interpola entre cores (exemplo: azul → amarelo → vermelho)
        Color heatColor = Color.Lerp(Color.green, Color.red, t);

        // Opcional: gradiente mais rico (passando pelo verde)
        if (t < 0.5f)
            heatColor = Color.Lerp(Color.green, Color.yellow, t * 2f);
        else
            heatColor = Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2f);

        quadRenderer.material.color = heatColor;
    }

    public void ResetCell()
    {
        discovered = false;
        visitCount = 0;
    }

}
