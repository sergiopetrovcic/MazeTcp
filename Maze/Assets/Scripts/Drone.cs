using UnityEngine;
using UnityEngine.InputSystem;

public class Drone : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody rb;
    public Transform propellerFL;
    public Transform propellerFR;
    public Transform propellerBL;
    public Transform propellerBR;

    [Header("Configurações")]
    public float hoverHeight = 1f;          // altura de hover inicial (m)
    public float hoverForce = 20f;          // força para manter o hover
    public float hoverDamping = 5f;         // suaviza a subida
    public float propellerRPM = 1000f;      // velocidade de rotação das hélices

    private bool hovering = false;
    private float visualRPMFiltered;

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    private bool IsKeyDown_Space() => Keyboard.current != null && Keyboard.current[Key.Space].wasPressedThisFrame;
    private bool IsKeyDown_Q() => Keyboard.current != null && Keyboard.current[Key.Q].wasPressedThisFrame;
    private bool IsKeyDown_E() => Keyboard.current != null && Keyboard.current[Key.E].wasPressedThisFrame;
#else
    private bool IsKeyDown_Space() => Input.GetKeyDown(KeyCode.Space);
    private bool IsKeyDown_Q() => Input.GetKeyDown(KeyCode.Q);
    private bool IsKeyDown_E() => Input.GetKeyDown(KeyCode.E);
#endif

    void Update()
    {
        HandleInput();
        UpdatePropellers();
    }

    void FixedUpdate()
    {
        if (!hovering) return;

        float currentHeight = transform.position.y;
        float error = hoverHeight - currentHeight;

        // força proporcional para manter o drone na altura alvo
        float upwardForce = (error * hoverForce) - (rb.linearVelocity.y * hoverDamping);
        rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
    }

    private void HandleInput()
    {
        if (IsKeyDown_Space())
            hovering = !hovering;

        if (IsKeyDown_Q())
        {
            hoverHeight = Mathf.Max(0f, hoverHeight - 0.1f); // diminui 10 cm, mínimo = 0
            Debug.Log($"Altura alvo: {hoverHeight:F2} m");
        }

        if (IsKeyDown_E())
        {
            hoverHeight += 0.1f; // aumenta 10 cm
            Debug.Log($"Altura alvo: {hoverHeight:F2} m");
        }
    }

    private void UpdatePropellers()
    {
        float targetRPM = hovering ? propellerRPM : 0f;
        visualRPMFiltered = Mathf.Lerp(visualRPMFiltered, targetRPM, 0.1f);

        float degreesPerSecond = visualRPMFiltered * 360f / 60f;

        RotateProp(propellerFL, degreesPerSecond);   // horário
        RotateProp(propellerFR, -degreesPerSecond);  // anti-horário
        RotateProp(propellerBL, -degreesPerSecond);  // anti-horário
        RotateProp(propellerBR, degreesPerSecond);   // horário
    }

    void RotateProp(Transform prop, float degPerSec)
    {
        if (!prop) return;
        prop.Rotate(Vector3.up, degPerSec * Time.deltaTime, Space.Self);
    }
}
