using UnityEngine;

public class HangingObject : MonoBehaviour, IResettable
{
    private HingeJoint2D hinge;
    private Rigidbody2D rb;

    private Vector3 startPos;
    private Quaternion startRot;

    private void Awake()
    {
        hinge = GetComponent<HingeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        LevelManager.instance.RegisterResettable(this);
    }

    public void Interact()
    {
        if (hinge != null)
            hinge.enabled = false; // 🔥 THIS CUTS THE ROPE
    }

    public void ResetState()
    {
        if (hinge != null)
            hinge.enabled = true; // re-tie the rope

        transform.position = startPos;
        transform.rotation = startRot;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}