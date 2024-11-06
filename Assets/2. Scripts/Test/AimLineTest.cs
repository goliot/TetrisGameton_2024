using UnityEngine;

public class AimLineTest : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform gunTip;
    public float maxDistance = 10f;
    public LayerMask obstacleLayer;  // 장애물 레이어 설정

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector2 aimDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - gunTip.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(gunTip.position, aimDirection, maxDistance, obstacleLayer);

        Vector2 endPoint = hit.collider != null ? hit.point : (Vector2)gunTip.position + aimDirection * maxDistance;

        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, endPoint);
    }
}
