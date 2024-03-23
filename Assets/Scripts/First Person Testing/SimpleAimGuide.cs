using UnityEngine;

public class SimpleAimGuide : MonoBehaviour
{
    [SerializeField] private float lineLength;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (lineRenderer.enabled)
        {
            lineRenderer.positionCount = 2;
            Vector3 direction = transform.parent.parent.forward;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + direction * lineLength);
        }
    }

    public void OnPickUp()
    {
        lineRenderer.enabled = true;
    }

    public void OnDrop()
    {
        lineRenderer.enabled = false;
    }
}
