using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Keeps the controller-selected button visible inside a horizontal ScrollRect.
/// Unity's ScrollRect does not follow UI selection by itself, so levels past the
/// edge of the viewport get selected off-screen. Put this on the "Scroll View" object.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 12f;

    private ScrollRect scroll;
    private GameObject lastSelected;
    private bool scrolling;

    private void Awake()
    {
        scroll = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        GameObject selected = es.currentSelectedGameObject;
        if (selected == null) return;

        if (selected != lastSelected)
        {
            lastSelected = selected;
            scrolling = true;
        }

        if (!scrolling) return;

        RectTransform target = selected.transform as RectTransform;
        RectTransform content = scroll.content;
        RectTransform viewport = scroll.viewport != null ? scroll.viewport : scroll.transform as RectTransform;

        if (target == null || content == null || !target.IsChildOf(content))
        {
            scrolling = false;
            return;
        }

        // Measure everything in the viewport's space. Content may be scaled (e.g. 2x),
        // and content.rect.width ignores that scale, so it can't be compared to the viewport.
        Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);
        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);

        float viewWidth = viewport.rect.width;
        float scrollable = contentBounds.size.x - viewWidth;
        if (scrollable <= 0f)
        {
            scrolling = false;
            return;
        }

        // Selected button's centre measured from the content's left edge.
        float centreX = targetBounds.center.x - contentBounds.min.x;

        float desired = Mathf.Clamp01((centreX - viewWidth * 0.5f) / scrollable);
        float current = scroll.horizontalNormalizedPosition;

        scroll.velocity = Vector2.zero; // stop drag inertia fighting the scroll
        scroll.horizontalNormalizedPosition =
            Mathf.Lerp(current, desired, Time.unscaledDeltaTime * smoothSpeed);

        if (Mathf.Abs(desired - current) < 0.001f)
        {
            scroll.horizontalNormalizedPosition = desired;
            scrolling = false;
        }
    }
}