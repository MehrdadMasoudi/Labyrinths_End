using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// Animator parameters: Float Speed, Bool IsCharging, Triggers Charge / Smash / Stomp.
// The boss must not be on the Ground or Wall layer.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class Minotaur : MonoBehaviour, IEnemyBrain
{
    public enum BossState
    {
        Idle,
        Walking,
        Charging,
        Smashing,
        Stomping,
        Recovering
    }
    public BossState CurrentState => currentState;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject shockwavePrefab;
    [Tooltip("Optional. If empty, shockwaves spawn at the bottom of the body collider.")]
    [SerializeField] private Transform shockwaveSpawnPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("The SOLID (non-trigger) collider.")]
    [SerializeField] private Collider2D bodyCollider;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Debug")]
    [SerializeField] private BossState currentState = BossState.Idle;
    [SerializeField] private bool debugLogs = false;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float facingDeadzone = 0.2f;

    [Header("Decision Making")]
    [SerializeField] private float attackDelayMin = 1f;
    [SerializeField] private float attackDelayMax = 2.2f;
    [Tooltip("Minimum walk time after recovering before a close-range attack.")]
    [SerializeField] private float minWalkTime = 0.6f;
    [SerializeField] private float closeRange = 4f;
    [SerializeField] private float midRange = 8f;
    [Tooltip("Weight multiplier for repeating the same attack (0 = never, 1 = no penalty).")]
    [Range(0f, 1f)][SerializeField] private float repeatPenalty = 0.25f;

    [Header("Pogo Response")]
    [Tooltip("How long a pogo counts toward the counter. Older pogos are forgotten.")]
    [SerializeField] private float pogoMemoryWindow = 3f;
    [Tooltip("Pogos needed to force a Stomp on the next decision.")]
    [SerializeField] private int pogosToForceStomp = 2;
    [Tooltip("Stomp weight added per remembered pogo while below the force threshold.")]
    [SerializeField] private float stompWeightPerPogo = 25f;
    [Tooltip("Sideways speed while walking out from under an overhead player.")]
    [SerializeField] private float overheadWalkSpeed = 3f;
    [Tooltip("Extra jump force per remembered pogo when Stomp launches. 0 = flat jumpForce.")]
    [SerializeField] private float stompForcePerPogo = 2f;

    [Header("Overhead Detection")]
    [Tooltip("Horizontal padding beyond the body collider used to decide 'overhead'.")]
    [SerializeField] private float overheadHorizontalPadding = 1.5f;
    [Tooltip("Vertical margin above the boss's head that still counts as overhead.")]
    [SerializeField] private float overheadVerticalMargin = 0.25f;

    [Header("Cornered Player")]
    [SerializeField] private float cornerCheckDistance = 3f;
    [SerializeField] private float cornerCheckHeight = 0.5f;
    [SerializeField] private float corneredStopDistance = 2.5f;
    [Range(0f, 1f)][SerializeField] private float corneredChargeMultiplier = 0.25f;
    [Tooltip("Speed at which the boss backs away while recovering from an attack on a cornered player.")]
    [SerializeField] private float backOffSpeed = 3f;

    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeWindup = 0.5f;
    [SerializeField] private float chargeMaxTime = 2f;
    [SerializeField] private float chargeRecoverTime = 0.8f;
    [Tooltip("Stun after hitting a wall. The player's damage window.")]
    [SerializeField] private float chargeWallStunTime = 1.3f;
    [Tooltip("Max damage instances per charge in NEUTRAL. 1 = fair single-hit contract.")]
    [SerializeField] private int chargeMaxHitsNeutral = 1;
    [Tooltip("Max damage instances per charge in RAGE.")]
    [SerializeField] private int chargeMaxHitsRage = 2;
    [Tooltip("Minimum time between damage instances in rage so a pinned player can't be shredded.")]
    [SerializeField] private float rageReHitCooldown = 0.35f;

    [Header("Smash")]
    [SerializeField] private float smashWindup = 0.4f;
    [SerializeField] private float smashRecoverTime = 1f;

    [Header("Stomp")]
    [SerializeField] private float stompWindup = 0.3f;
    [SerializeField] private float jumpForce = 15f;
    [FormerlySerializedAs("stompTrackSpeed")]
    [SerializeField] private float stompHorizontalSpeed = 6f;
    [Tooltip("Seconds in the air after launch before the slam. Must be larger than Stomp Lock Time.")]
    [SerializeField] private float stompHangTime = 0.55f;
    [Tooltip("End of the hang: the boss stops drifting and flashes red before slamming.")]
    [SerializeField] private float stompLockTime = 0.25f;
    [SerializeField] private float slamSpeed = 25f;
    [Tooltip("Safety net if landing is never detected.")]
    [SerializeField] private float stompMaxTime = 3f;
    [SerializeField] private float stompRecoverTime = 0.9f;

    [Header("Recovery")]
    [Tooltip("Rage can't shorten recovery below this.")]
    [SerializeField] private float minRecoverTime = 0.5f;

    [Header("Rage")]
    [Range(0f, 1f)][SerializeField] private float rageHealthPercent = 0.7f;
    [SerializeField] private float rageMultiplier = 1.5f;

    [Header("Colors")]
    [SerializeField] private Color telegraphColor = Color.red;
    [SerializeField] private Color rageColor = new Color(1f, 0.5f, 0.5f);

    [Header("Environment Checks")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float wallCheckDistance = 0.3f;

    [Header("Sleep / Wake")]
    [Tooltip("Boss is inert until the player enters this radius.")]
    [SerializeField] private float wakeRadius = 10f;
    [Tooltip("Optional. If empty, uses the sprite's base color at 50% alpha.")]
    [SerializeField] private Color sleepColor = new Color(1f, 1f, 1f, 0.5f);
    [Tooltip("If true, the 2s pause only runs after waking, not at scene load.")]
    [SerializeField] private bool delayAfterWake = true;
    [Tooltip("Optional Animator bool for a sleep pose. Leave empty to only tint the sprite.")]
    [SerializeField] private string isSleepingParamName = "";
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private static readonly int ChargeHash = Animator.StringToHash("Charge");
    private static readonly int SmashHash = Animator.StringToHash("Smash");
    private static readonly int StompHash = Animator.StringToHash("Stomp");
    private static readonly int IsSleepingHash = Animator.StringToHash("IsSleeping");

    private Rigidbody2D rb;
    private EnemyHealth health;
    private Color baseColor = Color.white;

    private float stateTimer;
    private float currentRecoverTime;
    private float currentAttackDelay;
    private bool isFacingRight = true;
    private bool isRaging;
    private bool deathHandled;
    private bool retreatWhileRecovering;
    private BossState lastAttack = BossState.Idle;

    private bool isSleeping = true;
    private bool hasWoken;
    private bool stompLaunched;
    private bool stompSlammed;
    private float stompVx;
    private int chargeHitCount;
    private float chargeLastHitTime;
    private float lastPogoTime = -999f;
    private int recentPogoCount;
    private int lastCountedPogoFrame = -1;
    private bool wasOverheadLastFrame;

    private float FacingDir => isFacingRight ? 1f : -1f;
    private float RageScale => isRaging ? rageMultiplier : 1f;
    public bool IsRaging => isRaging;
    public void DisableBrain() => enabled = false;
    public void EnableBrain() => enabled = true;
    public void GoToSleep()
    {
        hasWoken = false;
        EnterSleep();
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;

        if (bodyCollider == null)
        {
            foreach (var c in GetComponentsInChildren<Collider2D>())
            {
                if (!c.isTrigger) { bodyCollider = c; break; }
            }
        }

        if (bodyCollider == null)
        {
            Debug.LogError("MinotaurBoss: no solid (non-trigger) collider found. Assign Body Collider.", this);
            enabled = false;
        }

        if (corneredStopDistance >= closeRange)
        {
            Debug.LogWarning(
                $"Minotaur: corneredStopDistance ({corneredStopDistance}) should be < closeRange ({closeRange}), " +
                "otherwise Smash will be excluded when the player is near a wall.", this);
        }

        if (((1 << gameObject.layer) & (groundLayer | wallLayer)) != 0)
        {
            Debug.LogWarning(
                "Minotaur: boss is on a layer included in groundLayer or wallLayer. " +
                "Its own collider will satisfy IsGrounded / IsTouchingWall. Move the boss to its own layer.", this);
        }
    }

    void Start()
    {
        FindPlayer();
        EnterSleep();
    }

    private void EnterSleep()
    {
        isSleeping = true;
        ChangeState(BossState.Idle);
        StopHorizontal();

        if (animator != null && !string.IsNullOrEmpty(isSleepingParamName))
            animator.SetBool(IsSleepingHash, true);
        else if (spriteRenderer != null)
            spriteRenderer.color = sleepColor;

        if (debugLogs) Debug.Log("Boss: sleeping", this);
    }

    private void Wake()
    {
        if (hasWoken) return;
        hasWoken = true;
        isSleeping = false;

        if (animator != null && !string.IsNullOrEmpty(isSleepingParamName))
            animator.SetBool(IsSleepingHash, false);

        SetTelegraph(false); // restores base / rage colour

        if (debugLogs) Debug.Log("Boss: awake", this);

        if (delayAfterWake)
            StartCoroutine(WakeDelay());
        else
            ChangeState(BossState.Walking);
    }

    private IEnumerator WakeDelay()
    {
        ChangeState(BossState.Idle);
        yield return new WaitForSeconds(2f);
        if (currentState == BossState.Idle && !health.IsDead)
            ChangeState(BossState.Walking);
    }

    void Update()
    {
        if (health == null || health.IsDead)
        {
            if (health != null) HandleDeath();
            return;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Sleep / wake gate
        if (isSleeping)
        {
            if (Vector2.Distance(transform.position, player.position) <= wakeRadius)
                Wake();
            return;
        }

        CheckRage();

        if (animator != null)
            animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
    }

    void FixedUpdate()
    {
        if (health.IsDead || player == null) return;
        if (isSleeping) { StopHorizontal(); return; }

        switch (currentState)
        {
            case BossState.Idle: StopHorizontal(); break;
            case BossState.Walking: State_Walking(); break;
            case BossState.Charging: State_Charging(); break;
            case BossState.Smashing: State_Smashing(); break;
            case BossState.Stomping: State_Stomping(); break;
            case BossState.Recovering: State_Recovering(); break;
        }
    }
    private void FindPlayer()
    {
        var p = FindAnyObjectByType<PlayerHealth>();
        if (p != null) player = p.transform;
    }

    // ------------------------------------------------------------------
    // State machine core
    // ------------------------------------------------------------------

    private void ChangeState(BossState newState)
    {
        if (debugLogs) Debug.Log($"Boss: {currentState} -> {newState}", this);

        currentState = newState;
        stateTimer = 0f;
        stompLaunched = false;
        stompSlammed = false;
        SetTelegraph(false);

        if (newState == BossState.Charging)
        {
            chargeHitCount = 0;
            chargeLastHitTime = -999f;
        }

        if (newState == BossState.Walking) RollAttackDelay();

        if (animator != null)
        {
            animator.ResetTrigger(ChargeHash);
            animator.ResetTrigger(SmashHash);
            animator.ResetTrigger(StompHash);

            animator.SetBool(IsChargingHash, newState == BossState.Charging);

            if (newState == BossState.Charging) animator.SetTrigger(ChargeHash);
            if (newState == BossState.Smashing) animator.SetTrigger(SmashHash);
            if (newState == BossState.Stomping) animator.SetTrigger(StompHash);
        }

        if (newState == BossState.Charging ||
            newState == BossState.Smashing ||
            newState == BossState.Stomping)
        {
            lastAttack = newState;
            FacePlayer();
        }
    }

    private void StartRecovery(float duration)
    {
        currentRecoverTime = duration;
        retreatWhileRecovering = IsPlayerCornered();
        ChangeState(BossState.Recovering);
    }

    private void RollAttackDelay()
    {
        currentAttackDelay = Random.Range(attackDelayMin, attackDelayMax);
    }

    // ------------------------------------------------------------------
    // Charge damage gate (call this from your contact-damage script)
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns true if this contact should deal damage. Call once per contact
    /// event from your damage script. Enforces one hit per charge in neutral
    /// and a cooldown-gated second hit in rage.
    /// </summary>
    public bool TryConsumeChargeHit()
    {
        if (currentState != BossState.Charging) return true;

        int maxHits = isRaging ? chargeMaxHitsRage : chargeMaxHitsNeutral;
        if (chargeHitCount >= maxHits) return false;
        if (Time.time - chargeLastHitTime < rageReHitCooldown) return false;

        chargeHitCount++;
        chargeLastHitTime = Time.time;
        return true;
    }

    // ------------------------------------------------------------------
    // States
    // ------------------------------------------------------------------

    private void State_Walking()
    {
        stateTimer += Time.deltaTime;
        FacePlayer();

        float distX = Mathf.Abs(player.position.x - transform.position.x);
        bool overhead = IsPlayerOverhead();
        UpdatePogoCounter(overhead);

        bool cornered = IsPlayerCornered();
        float stopDist = cornered ? corneredStopDistance : stopDistance;

        // Walk out from under an overhead player instead of standing still.
        if (overhead)
        {
            float walkAway = isFacingRight ? -1f : 1f;
            if (IsTouchingWall(walkAway))
                StopHorizontal();
            else
                rb.linearVelocity = new Vector2(walkAway * overheadWalkSpeed * RageScale, rb.linearVelocity.y);
        }
        else if (distX > stopDist)
        {
            rb.linearVelocity = new Vector2(FacingDir * walkSpeed * RageScale, rb.linearVelocity.y);
        }
        else
        {
            StopHorizontal();
        }

        bool timeUp = stateTimer > currentAttackDelay;
        bool allowTimeUpAttack = timeUp && !overhead;
        bool playerClose = distX < closeRange && !overhead && stateTimer > minWalkTime;
        bool overheadReady = overhead && recentPogoCount > 0 && stateTimer > minWalkTime;

        if ((allowTimeUpAttack || playerClose || overheadReady) &&
            !TryPickAttack(distX, overhead, cornered))
        {
            stateTimer = 0f;
            RollAttackDelay();
        }
    }

    private bool TryPickAttack(float distX, bool overhead, bool cornered)
    {
        float wSmash = 0f, wStomp = 0f, wCharge = 0f, wNone = 0f;

        if (overhead)
        {
            // Stomp is the anti-air. Weight ramps with remembered pogos.
            wStomp = recentPogoCount * stompWeightPerPogo;
            wSmash = 15f;    // punishes a drop-in
            wCharge = 0f;    // can't reach an airborne player
            wNone = recentPogoCount == 0 ? 25f : 5f;
        }
        else
        {
            if (distX < closeRange) { wSmash = 45f; wStomp = 35f; wCharge = 20f; }
            else if (distX < midRange) { wSmash = 15f; wStomp = 35f; wCharge = 30f; wNone = 20f; }
            else { wSmash = 5f; wCharge = 60f; wStomp = 15f; wNone = 20f; }

            if (cornered) wCharge *= corneredChargeMultiplier;
        }

        if (lastAttack == BossState.Smashing) wSmash *= Mathf.Max(repeatPenalty, 0.5f);
        if (lastAttack == BossState.Stomping) wStomp *= repeatPenalty;
        if (lastAttack == BossState.Charging) wCharge *= repeatPenalty;

        // FORCE: two pogos → Stomp, no roll.
        if (overhead && recentPogoCount >= pogosToForceStomp)
        {
            if (debugLogs)
                Debug.Log($"Forced Stomp after {recentPogoCount} pogos", this);

            recentPogoCount = 0;
            ChangeState(BossState.Stomping);
            return true;
        }

        float total = wSmash + wStomp + wCharge + wNone;
        if (total <= 0f) return false;

        float roll = Random.value * total;

        if (debugLogs)
            Debug.Log($"PickAttack distX={distX:F2} overhead={overhead} cornered={cornered} " +
                      $"pogos={recentPogoCount} " +
                      $"wS={wSmash:F1} wSt={wStomp:F1} wC={wCharge:F1} wN={wNone:F1}", this);

        if ((roll -= wSmash) < 0f) { ChangeState(BossState.Smashing); return true; }
        if ((roll -= wStomp) < 0f) { recentPogoCount = 0; ChangeState(BossState.Stomping); return true; }
        if ((roll -= wCharge) < 0f) { ChangeState(BossState.Charging); return true; }
        return false;
    }

    private void State_Charging()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer < chargeWindup)
        {
            StopHorizontal();
            SetTelegraph(true);
            return;
        }
        SetTelegraph(false);

        float dir = FacingDir;
        rb.linearVelocity = new Vector2(dir * chargeSpeed * RageScale, rb.linearVelocity.y);

        if (IsTouchingWall(dir))
        {
            StopHorizontal();
            StartRecovery(chargeWallStunTime);
        }
        else if (stateTimer > chargeWindup + chargeMaxTime)
        {
            StopHorizontal();
            StartRecovery(chargeRecoverTime);
        }
    }

    private void State_Smashing()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer < smashWindup)
        {
            StopHorizontal();
            SetTelegraph(true);
            return;
        }

        SpawnShockwave();
        StartRecovery(smashRecoverTime);
    }

    private void State_Stomping()
    {
        stateTimer += Time.deltaTime;

        // Windup
        if (stateTimer < stompWindup)
        {
            StopHorizontal();
            SetTelegraph(true);
            return;
        }

        float slamTime = stompWindup + stompHangTime;
        float lockStart = slamTime - stompLockTime;

        // Launch: extra jump force per remembered pogo.
        if (!stompLaunched)
        {
            stompLaunched = true;
            SetTelegraph(false);

            float force = jumpForce + recentPogoCount * stompForcePerPogo;
            stompVx = Mathf.Sign(player.position.x - transform.position.x) * stompHorizontalSpeed;
            rb.linearVelocity = new Vector2(stompVx, force);
            return;
        }

        // Airborne: keep the committed direction
        if (!stompSlammed && stateTimer < lockStart)
        {
            SetTelegraph(false);
            rb.linearVelocity = new Vector2(stompVx, rb.linearVelocity.y);
        }
        // Lock: stop drifting and flash red
        else if (!stompSlammed && stateTimer < slamTime)
        {
            SetTelegraph(true);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Slam
        if (!stompSlammed && stateTimer >= slamTime)
        {
            stompSlammed = true;
            SetTelegraph(false);
            rb.linearVelocity = new Vector2(0f, -slamSpeed);
        }

        bool leftGround = stateTimer > stompWindup + 0.15f && rb.linearVelocity.y <= 0.1f;
        bool landed = leftGround && IsGrounded();
        bool timedOut = stateTimer > stompMaxTime;

        if (landed || timedOut)
        {
            if (landed) SpawnShockwave();
            StopHorizontal();
            StartRecovery(stompRecoverTime);
        }
    }

    private void State_Recovering()
    {
        stateTimer += Time.deltaTime;

        float awayDir = player.position.x > transform.position.x ? -1f : 1f;
        if (retreatWhileRecovering && !IsTouchingWall(awayDir))
            rb.linearVelocity = new Vector2(awayDir * backOffSpeed, rb.linearVelocity.y);
        else
            StopHorizontal();

        float duration = Mathf.Max(currentRecoverTime / RageScale, minRecoverTime);

        if (stateTimer > duration)
            ChangeState(BossState.Walking);
    }

    // ------------------------------------------------------------------
    // Pogo detection
    // ------------------------------------------------------------------

    // True if the player is above the boss (pogoing or standing on the head).
    // Based on horizontal proximity + height, not on the boss's pivot.
    private bool IsPlayerOverhead()
    {
        if (player == null || bodyCollider == null) return false;

        Bounds b = bodyCollider.bounds;
        float dx = Mathf.Abs(player.position.x - b.center.x);
        float headRoom = b.extents.x + overheadHorizontalPadding;

        bool horizontallyAbove = dx < headRoom;
        bool verticallyAbove = player.position.y > b.max.y - overheadVerticalMargin;

        return horizontallyAbove && verticallyAbove;
    }

    // Counts one pogo per overhead entry. Older pogos are forgotten after
    // pogoMemoryWindow seconds.
    private void UpdatePogoCounter(bool overhead)
    {
        if (overhead && !wasOverheadLastFrame && Time.frameCount != lastCountedPogoFrame)
        {
            lastCountedPogoFrame = Time.frameCount;
            lastPogoTime = Time.time;
            recentPogoCount++;

            if (debugLogs)
                Debug.Log($"Pogo counted: {recentPogoCount}", this);
        }

        wasOverheadLastFrame = overhead;

        if (Time.time - lastPogoTime > pogoMemoryWindow && recentPogoCount > 0)
        {
            recentPogoCount = 0;
            if (debugLogs) Debug.Log("Pogo memory expired", this);
        }
    }

    // ------------------------------------------------------------------
    // Rage / death
    // ------------------------------------------------------------------

    private void CheckRage()
    {
        if (isRaging || health.MaxHealth <= 0) return;

        float healthPercent = (float)health.currentHealth / health.MaxHealth;
        if (healthPercent <= rageHealthPercent) EnterRageMode();
    }

    private void EnterRageMode()
    {
        isRaging = true;
        SetTelegraph(false);
        if (debugLogs) Debug.Log("Minotaur is enraged!", this);
    }

    private void HandleDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        StopHorizontal();
        SetTelegraph(false);
        if (animator != null) animator.SetBool(IsChargingHash, false);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void SetTelegraph(bool on)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = on ? telegraphColor : (isRaging ? rageColor : baseColor);
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null) return;

        Vector3 pos = shockwaveSpawnPoint != null
            ? shockwaveSpawnPoint.position
            : new Vector3(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y, transform.position.z);

        Instantiate(shockwavePrefab, pos, Quaternion.identity);
    }

    private bool IsPlayerCornered()
    {
        if (player == null) return false;

        float awayDir = player.position.x >= transform.position.x ? 1f : -1f;
        Vector2 origin = (Vector2)player.position + Vector2.up * cornerCheckHeight;

        return Physics2D.Raycast(origin, new Vector2(awayDir, 0f), cornerCheckDistance, wallLayer | groundLayer).collider != null;
    }

    private bool IsGrounded()
    {
        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.1f);

        return Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    private bool IsTouchingWall(float dir)
    {
        Bounds b = bodyCollider.bounds;
        Vector2 size = new Vector2(b.size.x, b.size.y * 0.8f);

        return Physics2D.BoxCast(b.center, size, 0f, new Vector2(dir, 0f), wallCheckDistance, wallLayer | groundLayer).collider != null;
    }

    private void FacePlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < facingDeadzone) return;

        if (dx > 0f && !isFacingRight) Flip();
        else if (dx < 0f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }
}