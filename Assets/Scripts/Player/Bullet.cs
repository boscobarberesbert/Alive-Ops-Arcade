using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 1f;

    public void Setup(Vector3 shootDirection)
    {
        // Rotate bullet to shoot direction
        transform.forward = shootDirection.normalized;

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Destroy(collision.collider.gameObject);
        }

        Destroy(gameObject);
    }
}