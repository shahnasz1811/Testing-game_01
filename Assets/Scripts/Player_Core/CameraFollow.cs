using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;          // Drag your player here

    [Header("Settings")]
    public float smoothSpeed = 0.125f; // How smoothly the camera follows
    public Vector3 offset;             // Base offset from player (e.g. (0, 2, -10))

    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;   // How far ahead to look
    public float lookAheadSpeed = 0.1f;    // How quickly the look-ahead adjusts

    private Vector3 lookAheadOffset;
    private Rigidbody2D playerRb;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // Calculate look-ahead based on player velocity
        float moveDir = playerRb.linearVelocity.x;
        Vector3 targetLookAhead = new Vector3(moveDir * lookAheadDistance, 0, 0);

        // Smoothly interpolate look-ahead offset
        lookAheadOffset = Vector3.Lerp(lookAheadOffset, targetLookAhead, lookAheadSpeed);

        // Desired camera position = player + offset + look-ahead
        Vector3 desiredPosition = player.position + offset + lookAheadOffset;

        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
