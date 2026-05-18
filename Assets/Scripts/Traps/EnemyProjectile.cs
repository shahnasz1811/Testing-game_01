using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float direction;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float resetTime = 5f;
    private float lifetime;

    public void ActivateProjectile()
    {
        lifetime = 0;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float movementSpeed = speed * Time.deltaTime;
        transform.Translate(movementSpeed, 0, 0);
        //transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false); //When the projectile hits something, it deactivates itself.
    }
}