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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().Respawn();
        }

        Destroy(gameObject);
    }
}