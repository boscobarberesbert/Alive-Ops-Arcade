using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    // Shoot Members
    public Transform muzzlePoint; // Point from where it shoots
    public GameObject bullet;
    public float shootForce = 150f; // Bullet force

    public void Shoot(Vector3 aimDirection)
    {
        Vector3 targetPos;

        Ray ray = new Ray(muzzlePoint.position, aimDirection);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            targetPos = raycastHit.point;
        }
        else
            targetPos = ray.GetPoint(75); // Just a point far away from the player (missed)

        Vector3 shootDirection = targetPos - muzzlePoint.position;

        // Instantiate bullet in gun point
        GameObject currentBullet = Instantiate(bullet, muzzlePoint.position, Quaternion.identity);

        // Set properties of the bullet we have shot
        currentBullet.GetComponent<Bullet>().Setup(shootDirection);

        // Impulses the bullet to shoot direction
        currentBullet.GetComponent<Rigidbody>().AddForce(shootDirection.normalized * shootForce, ForceMode.Impulse);
    }
}