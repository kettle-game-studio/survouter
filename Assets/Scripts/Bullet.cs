using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime;
    public float speed = 10;
    public ParticleSystem explosionParticle;

    async void Start()
    {
        while (lifetime > 0)
        {
            await Awaitable.NextFrameAsync();
            transform.position += transform.forward * speed * Time.deltaTime;
            lifetime -= Time.deltaTime;
        }

        Instantiate(explosionParticle);
        explosionParticle.transform.position = transform.position;
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.collider.GetComponent<PlayerController>();
        if (player != null)
            player.Hit();
        lifetime = 0;
    }
}
