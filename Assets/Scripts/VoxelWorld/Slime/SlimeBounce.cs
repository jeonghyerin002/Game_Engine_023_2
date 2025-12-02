using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlimeBounce : MonoBehaviour
{
    public float jumpForce = 5f;
    public float moveRadius = 2f;
    public float moveForce = 2f;
    public float jumpIntervalMin = 0.8f;
    public float jumpIntervalMax = 1.4f;

    Rigidbody rb;
    Vector3 idleCenter;
    float nextJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        idleCenter = transform.position;
        ScheduleNextJump();
    }

    void Update()
    {
        // ÂËµæ ½ºÄÉÀÏ: ¼Óµµ¿¡ µû¶ó À§/¾Æ·¡·Î ÂîºÎ/´Ã¾î³²
        float vy = rb.velocity.y;
        float stretch = Mathf.Clamp(1f + (-vy * 0.1f), 0.6f, 1.4f);
        float squash = 1f / Mathf.Sqrt(stretch);
        transform.localScale = new Vector3(squash, stretch, squash);

        if (Time.time >= nextJumpTime)
        {
            TryJump();
            ScheduleNextJump();
        }
    }

    void ScheduleNextJump()
    {
        nextJumpTime = Time.time + Random.Range(jumpIntervalMin, jumpIntervalMax);
    }

    void TryJump()
    {
        if (!IsGrounded()) return;

        // Áß½É ±âÁØ ·£´ý ¹æÇâ
        Vector2 r = Random.insideUnitCircle.normalized;
        Vector3 horiz = new Vector3(r.x, 0f, r.y) * moveForce;

        rb.AddForce(horiz + Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        // ¾Æ·¡·Î Âª°Ô ·¹ÀÌ ½÷¼­ ¹Ù´Ú Ã¼Å©
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }
}
