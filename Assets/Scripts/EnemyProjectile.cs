using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float direction;
    [SerializeField] private float speed = 5f;

    public void SetDirection(float _direction)
    {
        direction = _direction;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}