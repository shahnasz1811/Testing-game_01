using UnityEngine;

public class HangingObject : MonoBehaviour
{
    private HingeJoint2D hinge;
    private Rigidbody2D rb;

    private void Awake()
    {
        hinge = GetComponent<HingeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Drop()
    {
        if (hinge != null)
            hinge.enabled = false; // 🔥 THIS CUTS THE ROPE
    }
}