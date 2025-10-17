using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Lidar : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Transform de onde o raio parte (a direção é o eixo Z).")]
    public Transform emitter;

    [Tooltip("Parte giratória do LiDAR (opcional, visual).")]
    public Transform lidarTransform;

    [Tooltip("Parte do motor (opcional, visual).")]
    public Transform lidarMotor;

    [Header("Rotação")]
    public float rotationSpeedRPS = 5f;
    public Vector3 rotationAxis = Vector3.up;
    public bool isActive = true;

    [Header("Medição")]
    public float maxRange = 30f;
    public float measurementInterval = 0.02f; // 50 Hz
    public int maxPoints = 300;

    [Header("Visualização no jogo")]
    public GameObject pointPrefab; // esfera pequena opcional
    public float pointScale = 0.05f;
    public Color hitColor = Color.green;
    public Color missColor = Color.red;
    public float laserWidth = 0.01f;
    public float laserGlow = 2f;

    private float nextMeasurementTime;
    private readonly List<GameObject> spawnedPoints = new List<GameObject>();
    private LineRenderer lineRenderer;

    void Start()
    {
        if (!emitter)
        {
            Debug.LogError("LIDAR: defina o 'emitter' no Inspector!");
            enabled = false;
            return;
        }

        // Configura o LineRenderer (para o feixe laser)
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = missColor;
        lineRenderer.startColor = missColor;
        lineRenderer.endColor = missColor;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    void Update()
    {
        if (!isActive) return;

        // Rotação visual (opcional)
        float degreesPerSecond = rotationSpeedRPS * 360f;
        if (lidarTransform)
            lidarTransform.Rotate(rotationAxis, degreesPerSecond * Time.deltaTime, Space.Self);
        if (lidarMotor)
            lidarMotor.Rotate(rotationAxis, degreesPerSecond * 5f * Time.deltaTime, Space.Self);

        // Medição periódica
        if (Time.time >= nextMeasurementTime)
        {
            FireRay();
            nextMeasurementTime = Time.time + measurementInterval;
        }
    }

    private void FireRay()
    {
        Vector3 origin = emitter.position;
        Vector3 direction = emitter.forward;
        Vector3 endPoint;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRange))
        {
            endPoint = hit.point;
            AddPoint(hit.point, hitColor);
            UpdateLaser(origin, endPoint, hitColor);
        }
        else
        {
            endPoint = origin + direction * maxRange;
            AddPoint(endPoint, missColor);
            UpdateLaser(origin, endPoint, missColor);
        }
    }

    private void AddPoint(Vector3 position, Color color)
    {
        // Remove pontos antigos se atingir o limite
        if (spawnedPoints.Count >= maxPoints)
        {
            Destroy(spawnedPoints[0]);
            spawnedPoints.RemoveAt(0);
        }

        GameObject point;
        if (pointPrefab)
        {
            point = Instantiate(pointPrefab, position, Quaternion.identity);
        }
        else
        {
            point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(point.GetComponent<Collider>());
            var mr = point.GetComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Unlit/Color"));
            mr.material.color = color;
        }

        point.transform.localScale = Vector3.one * pointScale;
        spawnedPoints.Add(point);
    }

    private void UpdateLaser(Vector3 origin, Vector3 endPoint, Color color)
    {
        if (!lineRenderer) return;

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPoint);

        // cor e brilho
        lineRenderer.startColor = color * laserGlow;
        lineRenderer.endColor = color * laserGlow;
        lineRenderer.material.color = color;
    }
}
