using UnityEngine;

/// <summary>
/// Simple camera controller for pan and zoom over the hex grid.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    private Camera cam;
    private Vector3 dragOrigin;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
        HandleRotation();
    }

    private void HandlePan()
    {
        // Pan with WASD or Arrow keys
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 movement = (right * horizontal + forward * vertical) * panSpeed * Time.deltaTime;
        transform.position += movement;

        // Pan with middle mouse button drag
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            difference.y = 0; // Keep on horizontal plane
            transform.position += difference;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            Vector3 position = transform.position;
            position.y -= scroll * zoomSpeed;
            position.y = Mathf.Clamp(position.y, minZoom, maxZoom);
            transform.position = position;
        }
    }

    private void HandleRotation()
    {
        // Rotate with Q and E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
