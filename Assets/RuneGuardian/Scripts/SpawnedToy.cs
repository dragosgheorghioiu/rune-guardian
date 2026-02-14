using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using RuneGuardian;

/// <summary>
/// Represents an individual toy spawned in the world. 
/// Handles its own state, movement, and interaction with magic spells.
/// </summary>
public class SpawnedToy : MonoBehaviour
{
    /// <summary>
    /// Triggered when the toy is despawned (either repaired or missed).
    /// </summary>
    public static Action onToyDespawn;

    public Action OnArrivedTarget;
    public Action onToyHit;
    public Action OnStartedDespawn;
    public Action onToyRepaired;

    /// <summary>
    /// The type of projectile required to "fix" this toy.
    /// </summary>
    [Header("Cu ce spell trebuie sa fie lovit")]
    public ProjectileType requiredProjectile;

    [Header("Spell Symbol Display")]
    [SerializeField] private Transform symbolAnchor; // Where to attach the symbol (above toy)
    [SerializeField] private float symbolYOffset = 1.5f; // Vertical offset above toy
    [SerializeField] private float symbolScale = 0.3f; // Scale of the symbol
    private GameObject spawnedSymbol; // The instantiated symbol

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
    private bool isSphereMode;

    private enum State { MovingToPortal, MovingToTarget, WaitingAtTarget, MovingBackToPortal, MovingToDespawn }
    private State state;


    private CanvasGroup canvasGroup;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private float fadeStartDistance = 1.0f;

    private void Awake()
    {
        if (normalModel != null) normalModel.SetActive(false);
        if (hitModel != null) hitModel.SetActive(false);
    }

    public void Init(GameMode gameMode, Transform tPoint, Transform dPoint, Transform pPoint)
    {
        isSphereMode = gameMode == GameMode.SPHERE;
        targetPoint = tPoint;
        despawnPoint = dPoint;
        portalPoint = pPoint;

        if (variantAnchor == null) variantAnchor = transform;

        if (portalPoint != null)
        {
            state = State.MovingToPortal;
        }
    }

    /// <summary>
    /// Attaches a spell symbol above the toy to show which gesture is needed.
    /// </summary>
    public void AttachSpellSymbol(GameObject symbolPrefab)
    {
        if (symbolPrefab == null) return;

        // Determine the anchor point
        Transform anchor = symbolAnchor != null ? symbolAnchor : transform;

        // Calculate position above the toy
        Vector3 symbolPosition = anchor.position + Vector3.up * symbolYOffset;

        // Instantiate the symbol (flipped to face the player)
        spawnedSymbol = Instantiate(symbolPrefab, symbolPosition, Quaternion.Euler(90f, 180f, 0f));

        // Parent it to the toy so it moves with it
        spawnedSymbol.transform.SetParent(anchor);

        // Scale it down (10x smaller)
        spawnedSymbol.transform.localScale = Vector3.one * symbolScale * 0.1f;
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
        if (!isSphereMode && projectileType != requiredProjectile) return;

        onToyHit?.Invoke();
        await Task.Delay(1000);

        SwapToHitVariant();
        onToyRepaired?.Invoke();

        // Hide the spell symbol after repair
        if (spawnedSymbol != null)
        {
            Destroy(spawnedSymbol);
            spawnedSymbol = null;
        }

        // Record successful toy delivery
        GameStats.RecordToyDelivered();

        await Task.Delay(2500);

        state = State.MovingBackToPortal;
        OnStartedDespawn?.Invoke();
    }

    public bool IsCorrectSpell(ProjectileType projectileType)
    {
        return isSphereMode || (state == State.WaitingAtTarget && projectileType == requiredProjectile);
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