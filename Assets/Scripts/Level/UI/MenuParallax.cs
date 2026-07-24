using UnityEngine;

// Attach to a parent GameObject in your main menu, then assign your 3 layers
// in the Layers list below (works for either UI Image RectTransforms or
// plain SpriteRenderer Transforms - both are Transforms under the hood, this
// only ever touches localPosition).
//
// As the mouse moves around the screen, each layer drifts from its resting
// position based on the mouse's offset from screen center. Give background
// layers a small Strength and foreground layers a bigger one - the
// difference in how far each one moves is what reads as depth/parallax.
//
// IMPORTANT: each layer's image needs to be sized bigger than the
// screen/canvas so the drifting never reveals empty space at the edges -
// how much bigger depends on Strength * Max Offset for that layer.
public class MenuParallax : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform layerTransform;
        [Tooltip("How far this layer drifts relative to the others. Try ~0.2 for background, ~0.5 for midground, ~1 for foreground.")]
        public float strength = 0.5f;

        [System.NonSerialized] public Vector3 restPosition;
    }

    [SerializeField] private Layer[] layers;
    [Tooltip("How quickly layers catch up to the mouse - higher = snappier, lower = smoother/laggier.")]
    [SerializeField] private float smoothSpeed = 5f;
    [Tooltip("Max distance (in the same units as the layer's localPosition, e.g. pixels for UI) any layer can drift, regardless of strength.")]
    [SerializeField] private float maxOffset = 50f;

    private void Start()
    {
        foreach (Layer layer in layers)
        {
            if (layer.layerTransform != null)
                layer.restPosition = layer.layerTransform.localPosition;
        }
    }

    private void Update()
    {
        // Mouse position relative to screen center, roughly -1..1 on each axis.
        Vector2 normalizedMouse = new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        );

        foreach (Layer layer in layers)
        {
            if (layer.layerTransform == null) continue;

            Vector3 offset = new Vector3(
                Mathf.Clamp(normalizedMouse.x * layer.strength * maxOffset, -maxOffset, maxOffset),
                Mathf.Clamp(normalizedMouse.y * layer.strength * maxOffset, -maxOffset, maxOffset),
                0f
            );

            Vector3 targetPosition = layer.restPosition + offset;
            layer.layerTransform.localPosition = Vector3.Lerp(layer.layerTransform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }
}
