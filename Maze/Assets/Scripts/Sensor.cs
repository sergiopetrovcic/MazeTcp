using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Sensor : MonoBehaviour
{
    public float distance;// = 1f;
    public LayerMask detectLayers;// = ~0;
    public bool drawGizmos = true;
    public float originOffset = 0.55f; // metade do cubo
    public bool isDetecting { get; private set; }
    public RaycastHit hitInfo { get; private set; }

    void Update()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + transform.forward * originOffset;
        isDetecting = Physics.Raycast(origin, transform.forward, out hit, distance, detectLayers);
        hitInfo = hit;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Vector3 origin = transform.position + transform.forward * originOffset;
        Gizmos.color = isDetecting ? Color.red : Color.green;
        Gizmos.DrawRay(origin, transform.forward * distance);
    }
}