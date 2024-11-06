using UnityEngine;

public class AimRaycast : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField]
    private GameObject crosshair;      // ������ �ν��Ͻ�

    public float maxDistance = 10f;
    public LayerMask obstacleLayer;
    public Transform gunTip;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        obstacleLayer = LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        if (!gunTip) return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, gunTip.position.z - Camera.main.transform.position.z));
        Vector2 aimDirection = (mouseWorldPosition - gunTip.position).normalized;
        float distanceToMouse = Vector2.Distance(gunTip.position, mouseWorldPosition);

        RaycastHit2D hit = Physics2D.Raycast(gunTip.position, aimDirection, Mathf.Min(maxDistance, distanceToMouse), obstacleLayer);
        Vector3 endPoint = hit.collider != null ? (Vector3)hit.point : mouseWorldPosition;

        // ���� �������� ���� �� �� ��ġ ����
        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, endPoint);

        // ������ ��ġ ����
        if (crosshair != null)
        {
            crosshair.transform.position = mouseWorldPosition;
        }
    }

    public void SetGunTip(Transform target)
    {
        gunTip = target;
    }
}
