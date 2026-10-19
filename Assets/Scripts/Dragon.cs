using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Dragon : MonoBehaviour
{
    public Arena arena;
    public NavMeshAgent navMeshAgent;
    public Rigidbody body;
    public Animator animator;
    public PlayerController _player;
    public Bullet fireBullet;
    public Transform bulletSpawnPoint;

    public bool getShot;

    async void Start()
    {
        _player = arena.GetPlayer();
        await DragonCoroutine();
    }

    void Update()
    {
        animator.SetFloat("WalkSpeed", navMeshAgent.velocity.magnitude);
    }

    async Awaitable DragonCoroutine()
    {
        await StartFlexing();
        while (true)
        {
            await Flexing();
            await Chepyking();
        }
    }

    async Awaitable StartFlexing()
    {
        while (!getShot) await Awaitable.NextFrameAsync();
        getShot = false;

        animator.SetBool("Unpack", true);
        _player.CameraShake(1f);
        await FlyTo(arena.NextFlyingSpot(), 20f);
    }

    async Awaitable Flexing()
    {
        while (true)
        {
            var wasShot = await FlyTo(arena.NextFlyingSpot(), 15f, true);
            if (wasShot)
            {
                Debug.Log("AAaaaa");
                await Fall();
                return;
            }

            if (Random.Range(0, 10) > 6)
            {
                animator.SetBool("Fire", true);
                await Awaitable.WaitForSecondsAsync(0.5f);
                var spawnedBullet = Instantiate(fireBullet);
                spawnedBullet.transform.position = bulletSpawnPoint.position;
                spawnedBullet.transform.rotation = bulletSpawnPoint.rotation;
                await Awaitable.WaitForSecondsAsync(1.0f);
            }
            else if (Random.Range(0, 10) > 7)
            {
                await Fall(true);
                return;
            }
        }
    }

    async Awaitable Chepyking()
    {
        navMeshAgent.enabled = true;
        body.isKinematic = true;

        while (true)
        {
            navMeshAgent.destination = _player.transform.position;
            if (Vector3.Distance(_player.transform.position, transform.position) < 7)
            {
                animator.SetBool("Fire", true);
                await Awaitable.WaitForSecondsAsync(1.5f);
            }
            else if (Random.Range(0f, 1f) < 0.1 * Time.deltaTime)
            {
                animator.SetBool("Ground", false);
                navMeshAgent.enabled = false;
                body.isKinematic = false;
                return;
            }

            await Awaitable.NextFrameAsync();
        }
    }

    async Awaitable Fall(bool gracefully = false)
    {
        if (Physics.Raycast(transform.position + Vector3.down * 5, Vector3.down * 1000, out var hitInfo))
        {
            if (!gracefully)
                animator.SetBool("Fall", true);
            else
                animator.SetBool("Ground", true);

            var point = hitInfo.point;

            var time = 1f;
            var startPosition = transform.position;
            var startTime = Time.time;

            while (Time.time < startTime + time)
            {
                var delta = (Time.time - startTime) / time;

                transform.position = Vector3.Lerp(startPosition, point, delta);

                await Awaitable.NextFrameAsync();
            }

            if (!gracefully)
                await Awaitable.WaitForSecondsAsync(2f);

            animator.SetBool("Ground", true);
            animator.SetBool("Fall", false);
        }
    }

    async Awaitable<bool> FlyTo(Transform to, float speed, bool canBeShot = false)
    {
        var time = Vector3.Distance(transform.position, to.position) / speed;

        var startPosition = transform.position;
        var startRotation = transform.rotation;
        var startTime = Time.time;

        var direction = to.position - startPosition;
        var targetRotation = Quaternion.LookRotation(direction);

        while (Time.time < startTime + time)
        {
            var delta = (Time.time - startTime) / time;
            transform.position = Vector3.Lerp(startPosition, to.position, delta);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Mathf.Pow(delta, 0.7f));

            await Awaitable.NextFrameAsync();

            if (getShot)
            {
                getShot = false;
                if (canBeShot)
                    return true;
            }
        }

        return false;
    }
}
