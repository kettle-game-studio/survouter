using UnityEngine;
using UnityEngine.AI;

public class Dragon : MonoBehaviour
{
    public Arena arena;
    public NavMeshAgent navMeshAgent;
    public Animator animator;

    public bool startFlexing;
    public bool getShot;

    async void Start()
    {
        await DragonCoroutine();
    }

    async Awaitable DragonCoroutine()
    {
        await StartFlexingCoroutine();
        await FlexingCoroutine();
    }

    async Awaitable StartFlexingCoroutine()
    {
        while (!startFlexing) await Awaitable.NextFrameAsync();

        animator.SetBool("Unpack", true);
        await FlyTo(arena.NextFlyingSpot(), 20f);
    }

    async Awaitable<bool> FlexingCoroutine()
    {
        while (true)
        {
            var wasShot = await FlyTo(arena.NextFlyingSpot(), 15f, true);
            if (wasShot)
            {
                Debug.Log("AAaaaa");
                return true;
            }
            if (Random.Range(0, 10) > 6)
            {
                animator.SetBool("Fire", true);
            }
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
                if (!canBeShot)
                    continue;

                return true;
            }
        }

        return false;
    }
}
