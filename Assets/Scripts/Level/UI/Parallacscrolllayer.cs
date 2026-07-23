using UnityEngine;
using UnityEngine.UI;

// Put this on a layer that sits OUTSIDE the ScrollRect's Content (e.g. a
// sibling of Content, inside Viewport) - anything parented INSIDE Content
// already moves 1:1 with the scroll for free and doesn't need this script.
//
// Hierarchy example:
//   Scroll View (ScrollRect)
//     Viewport
//       Background   <- this script lives here, parallaxFactor ~0.3-0.5
//       Content
//         Foreground <- just parent it here directly, moves 1:1, no script
//         1, 2, Image, Image (1), ...
[RequireComponent(typeof(RectTransform))]
public class ParallaxScrollLayer : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    [Tooltip("0 = layer doesn't move at all (feels far away). 1 = moves exactly with the content, same as parenting it inside Content.")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.4f;

    private RectTransform layerRect;
    private RectTransform contentRect;
    private Vector2 layerStartPos;
    private Vector2 contentStartPos;

    private void Awake()
    {
        layerRect = (RectTransform)transform;
    }

    private void Start()
    {
        contentRect = scrollRect.content;
        layerStartPos = layerRect.anchoredPosition;
        contentStartPos = contentRect.anchoredPosition;

        scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    private void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    private void OnScrollChanged(Vector2 _)
    {
        Vector2 contentDelta = contentRect.anchoredPosition - contentStartPos;
        layerRect.anchoredPosition = layerStartPos + contentDelta * parallaxFactor;
    }
}