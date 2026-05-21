using UnityEngine;

public class Box : MonoBehaviour, IResettable
{
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        GameManager.instance.RegisterResettable(this);
    }

    public void ResetState()
    {
        transform.position = startPos;
        gameObject.SetActive(true);
    }
}