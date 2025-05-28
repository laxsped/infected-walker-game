using UnityEngine;

public class ZombieImpactSound : MonoBehaviour
{
    public AudioClip impactSound;
    public float forceThreshold = 1.5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > forceThreshold && impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, collision.contacts[0].point);
        }
    }
}
