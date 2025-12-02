using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target")]
    public VoxelWorld voxelWorld;

    [Header("Orbit Settings")]
    public float distance = 50f;
    public float height = 30f;
    public float rotationSpeed = 20f;

    [Header("Manual Control")]
    public bool autoRotate = true;
    public float manualRotateSpeed = 100f;
    public float zoomSpeed = 10f;
    public float minDistance = 10f;
    public float maxDistance = 100f;

    private Vector3 targetCenter;
    private float currentAngle = 0f;

    void Start()
    {
        if (voxelWorld != null)
        {
            // 맵 중심 계산
            targetCenter = new Vector3(
                voxelWorld.width / 2f,
                voxelWorld.maxHeight / 2f,
                voxelWorld.depth / 2f
            );
        }
        else
        {
            targetCenter = Vector3.zero;
        }

        UpdateCameraPosition();
    }

    void Update()
    {
        // 자동 회전
        if (autoRotate)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
        }

        // 수동 회전 (좌우 화살표 또는 A/D)
        float manualInput = Input.GetAxis("Horizontal");
        if (manualInput != 0)
        {
            autoRotate = false;
            currentAngle += manualInput * manualRotateSpeed * Time.deltaTime;
        }

        // 줌 (마우스 휠)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 스페이스바로 자동 회전 토글
        if (Input.GetKeyDown(KeyCode.Space))
        {
            autoRotate = !autoRotate;
        }

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        float radian = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Sin(radian) * distance,
            height,
            Mathf.Cos(radian) * distance
        );

        transform.position = targetCenter + offset;
        transform.LookAt(targetCenter);
    }

    // Inspector에서 voxelWorld 없이 수동 설정
    public void SetTarget(Vector3 center)
    {
        targetCenter = center;
    }
}