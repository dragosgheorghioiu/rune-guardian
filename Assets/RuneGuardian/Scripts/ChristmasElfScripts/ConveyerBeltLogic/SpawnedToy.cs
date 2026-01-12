using System;
using UnityEngine;
using System.Threading.Tasks;

public class SpawnedToy : MonoBehaviour
{
    public static Action onToyDespawn;
    public static Action onToyHit;
    public static Action onToyRepaired;

    [Header("Cu ce spell trebuie sa fie lovit")]
    public ProjectileType requiredProjectile;

    [SerializeField] private GameObject normalModel;
    [SerializeField] private GameObject hitModel;

    [Tooltip("Where to instantiate other variant (default e acest transform).")]
    public Transform variantAnchor;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float arriveDistance = 0.05f;

    private Transform targetPoint;
    private Transform despawnPoint;

    private enum State { MovingToTarget, WaitingAtTarget, MovingToDespawn }
    private State state;

    private GameObject _currentVariantInstance;

    public System.Action OnArrivedTarget;
    public System.Action OnStartedDespawn;

    private void Awake()
    {
        if (normalModel != null) normalModel.SetActive(true);
        if (hitModel != null) hitModel.SetActive(false);
    }

    public void Init(Transform tPoint, Transform dPoint)
    {
        targetPoint = tPoint;
        despawnPoint = dPoint;
        state = State.MovingToTarget;

        if (variantAnchor == null) variantAnchor = transform;
    }

    private void Update()
    {
        if (targetPoint == null || despawnPoint == null) return;

        if (state == State.MovingToTarget)
        {
            MoveTowards(targetPoint.position);

            if (Vector3.Distance(transform.position, targetPoint.position) <= arriveDistance)
            {
                state = State.WaitingAtTarget;
                OnArrivedTarget?.Invoke();
            }
        }
        else if (state == State.MovingToDespawn)
        {
            MoveTowards(despawnPoint.position);

            if (Vector3.Distance(transform.position, despawnPoint.position) <= arriveDistance)
            {
                Destroy(gameObject);
                onToyDespawn?.Invoke();
            }
        }
    }

    private void MoveTowards(Vector3 dest)
    {
        transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
    }

    async public void TryHit(ProjectileType projectileType)
    {
        // ignora deocamdata proiectilul daca nu a ajuns la target
        if (state != State.WaitingAtTarget)
            return;

        // daca nu e lovit de proiectilul asociat
        if (projectileType != requiredProjectile)
            return;

        // hit by correct projectile
        onToyHit?.Invoke();
        await Task.Delay(1000);

        SwapToHitVariant();
        onToyRepaired?.Invoke();

        await Task.Delay(2500);
        state = State.MovingToDespawn;

        OnStartedDespawn?.Invoke();
    }
    
    public bool IsCorrectSpell(ProjectileType projectileType)
    {
        return state == State.WaitingAtTarget && projectileType == requiredProjectile;
    }

    private void SwapToHitVariant()
    {
        if (normalModel != null) normalModel.SetActive(false);
        if (hitModel != null) hitModel.SetActive(true);
    }
}
