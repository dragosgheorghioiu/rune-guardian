using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

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
    private Transform portalPoint;

    private enum State { MovingToPortal, MovingToTarget, WaitingAtTarget, MovingBackToPortal, MovingToDespawn }
    private State state;

    public System.Action OnArrivedTarget;
    public System.Action OnStartedDespawn;

    private CanvasGroup canvasGroup;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private float fadeStartDistance = 1.0f;

    private void Awake()
    {
        if (normalModel != null) normalModel.SetActive(false);
        if (hitModel != null) hitModel.SetActive(false);
    }

    public void Init(Transform tPoint, Transform dPoint, Transform pPoint)
    {
        targetPoint = tPoint;
        despawnPoint = dPoint;
        portalPoint = pPoint;
        
        if (variantAnchor == null) variantAnchor = transform;

        if (portalPoint != null)
        {
            state = State.MovingToPortal;
        }
    }
    
    private void Update()
    {
        if (targetPoint == null || despawnPoint == null) return;

        if (state == State.MovingToPortal)
        {
            MoveTowards(portalPoint.position);
            float distanceToPortal = Vector3.Distance(transform.position, portalPoint.position);

            if (distanceToPortal <= fadeStartDistance && !isFadingIn)
            {
                isFadingIn = true;
                StartCoroutine(FadeVisibility(true));
            }

            if (distanceToPortal <= arriveDistance)
            {
                state = State.MovingToTarget;
            }
        }
        else if (state == State.MovingToTarget)
        {
            MoveTowards(targetPoint.position);

            if (Vector3.Distance(transform.position, targetPoint.position) <= arriveDistance)
            {
                state = State.WaitingAtTarget;
                OnArrivedTarget?.Invoke();
            }
        }
        else if (state == State.MovingBackToPortal)
        {
            MoveTowards(portalPoint.position);
            float distanceToPortal = Vector3.Distance(transform.position, portalPoint.position);

            if (distanceToPortal <= fadeStartDistance && !isFadingOut)
            {
                isFadingOut = true;
                StartCoroutine(FadeVisibility(false));
            }

            if (distanceToPortal <= arriveDistance)
            {
                state = State.MovingToDespawn;
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
        if (state != State.WaitingAtTarget) return;
        if (projectileType != requiredProjectile) return;

        onToyHit?.Invoke();
        await Task.Delay(1000);

        SwapToHitVariant();
        onToyRepaired?.Invoke();

        await Task.Delay(2500);

        state = State.MovingBackToPortal;
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

    private IEnumerator FadeVisibility(bool fadeIn)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        if (fadeIn)
        {
            if (normalModel != null) normalModel.SetActive(true);
            if (hitModel != null) hitModel.SetActive(false);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = fadeIn ? (elapsed / duration) : (1f - elapsed / duration);

            if (normalModel != null)
            {
                var canvasGroup = normalModel.GetComponent<CanvasGroup>();
                if (canvasGroup != null) canvasGroup.alpha = alpha;
            }

            yield return null;
        }

        if (!fadeIn)
        {
            if (normalModel != null) normalModel.SetActive(false);
            if (hitModel != null) hitModel.SetActive(false);
        }
    }
}