using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет поведением охранника: патруль, тревога, преследование и возврат.
/// </summary>
public class GuardPatrol : MonoBehaviour
{
    public enum State { Walking, Alerted, Searching, Pursuing, Returning }

    // ─── Инспектор ────────────────────────────────────────────────────────────
    #region Inspector fields

    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private Transform playerTransform;

    #endregion

    // ─── Константы ────────────────────────────────────────────────────────────
    #region Constants

    private const float BREADCRUMB_DISTANCE = 2f;
    private const int MAX_BREADCRUMBS = 500;
    private const float LOOK_PHASE_DURATION = 0.7f;
    private const float SCAN_ROTATION_SPEED = 2.5f;
    private const float DIR_SMOOTH_SPEED = 6f;

    // Движение
    private const float SPEED_WALK = 2f;
    private const float SPEED_RUN = 3.5f;
    private const float SPEED_ROTATE = 10f;

    // Дистанции
    private const float CHASE_BREAK_RADIUS = 12f;   // дистанция разрыва преследования
    private const float MELEE_KILL_RADIUS = 1.2f;  // мгновенная смерть при касании
    private const float WAYPOINT_ARRIVE_DIST = 0.1f;  // «дошёл до точки»
    private const float RETURN_ARRIVE_DIST = 0.8f;  // «дошёл до крошки»

    // Таймеры / углы
    private const float SEARCH_DURATION = 3f;
    private const float ALERT_DELAY = 0.2f;
    private const float LOOK_ANGLE = 45f;
    private const float PURSUIT_LOST_TIMEOUT = 4f;
    private const float WAYPOINT_STUCK_TIME = 2f;
    private const float TURN_TO_PLAYER_DELAY = 0.05f;
    private const float GAMEOVER_TURN_TIME = 0.3f;

    // Физика
    private const float GROUND_FOLLOW_HEIGHT = 0.1f;
    private const float WATER_SUBMERGE_DEPTH = 0.4f;
    private const float GROUND_SMOOTH_SPEED = 10f;
    private const float GUARD_PUSH_RADIUS = 1.0f;
    private const float GUARD_MIN_DIST = 0.1f;
    private const float GUARD_PUSH_STEP = 0.01f;
    private const float WALL_CHECK_DIST = 0.6f;
    private const float CLEAR_PATH_CAST_DIST = 2.5f;
    private const float STEERING_ARRIVE_DIST = 0.4f;

    // Анимация
    private const float ANIM_SPEED_THRESHOLD = 0.15f;

    #endregion

    // ─── Внутреннее состояние ─────────────────────────────────────────────────
    #region Runtime state

    private State currentState = State.Walking;
    private int currentPointIndex = 0;
    private float stateTimer = 0f;

    private Vector3 lastSeenPlayerPosition;
    private bool isHiding = false;

    // Поворот к игроку перед смертью
    private Quaternion targetRotationToPlayer;
    private float turnStartTime;
    private bool isTurningToPlayer;

    // Преследование
    private float pursuitLostTimer = 0f;

    // Осмотр по сторонам
    private bool isLookingAround = false;
    private float lookTimer = 0f;
    private Quaternion lookStartRot;

    // Хлебные крошки (возвратный путь)
    private List<Vector3> breadcrumbs = new List<Vector3>();
    private List<Vector3> returnPath = new List<Vector3>();
    private Vector3 lastBreadcrumbPos;

    // Зависание у точки маршрута
    private float waypointStuckTimer = 0f;
    private Vector3 lastPosForWaypointStuck;

    // Движение
    private Vector3 smoothMoveDir;
    private Vector3 cachedDirectionToTarget;
    private Vector3 lastAnimPos;

    // Ссылки
    private GameOver gameOver;
    private List<GuardPatrol> allGuards = new List<GuardPatrol>();
    private LayerMask raycastMask;

    // Материалы поля зрения
    private Material fovGreen;
    private Material fovYellow;
    private Material fovRed;
    #endregion

    // ─── Инициализация ────────────────────────────────────────────────────────
    #region Unity lifecycle

    private void Awake()
    {
        fovGreen = Resources.Load<Material>("FOV_mat/FOV_Walking");
        fovYellow = Resources.Load<Material>("FOV_mat/FOV_Alert");
        fovRed = Resources.Load<Material>("FOV_mat/FOV_Danger");
    }

    private void Start()
    {
        gameOver = FindAnyObjectByType<GameOver>();
        lastBreadcrumbPos = transform.position;
        lastAnimPos = transform.position;
        raycastMask = ~LayerMask.GetMask("Guard");
        allGuards = new List<GuardPatrol>(FindObjectsByType<GuardPatrol>(FindObjectsSortMode.None));
    }

    private void Update()
    {
        fieldOfView?.UpdateFOV(transform.position, transform.forward);

        PlayerCheck();
        CurrentStateCheck();

        if (isTurningToPlayer)
            UpdateTurnToPlayer();

        UpdateAnimator();
        FollowGround();
        AvoidGuards();
        ResolveWallClipping();
    }

    #endregion

    // ─── Диспетчер состояний ──────────────────────────────────────────────────
    #region State dispatch

    /// <summary>
    /// Проверка текущего состояния охранника
    /// </summary>
    private void CurrentStateCheck()
    {
        switch (currentState)
        {
            case State.Walking:
                HandleWalking();
                fieldOfView?.SetMaterial(fovGreen);
                break;

            case State.Pursuing:
                HandlePursuing();
                fieldOfView?.SetMaterial(fovYellow);
                break;

            case State.Returning:
                HandleReturning();
                fieldOfView?.SetMaterial(fovYellow);
                break;

            case State.Alerted:
            case State.Searching:
                HandleAlertedOrSearching();
                fieldOfView?.SetMaterial(fovYellow);
                break;
        }
    }

    #endregion

    // ─── Обнаружение игрока ───────────────────────────────────────────────────
    #region Player detection

    /// <summary>
    /// Каждый кадр проверяет поле зрения и реагирует на результат.
    /// </summary>
    private void PlayerCheck()
    {
        FieldOfView.DetectionType detection = fieldOfView.CheckForDetection();

        if (detection == FieldOfView.DetectionType.InstantDeath)
        {
            lastSeenPlayerPosition = fieldOfView.PlayerTransform.position;
            StartTurnAndDie(lastSeenPlayerPosition);
            return;
        }

        if (detection == FieldOfView.DetectionType.None)
        {
            TryResetHiding();

            if (currentState != State.Pursuing)
                return;
        }

        if (currentState == State.Searching)
        {
            StartTurnAndDie(lastSeenPlayerPosition);
            return;
        }

        if (isHiding && currentState != State.Pursuing)
            return;

        if (playerTransform != null)
            lastSeenPlayerPosition = playerTransform.position;

        if (currentState != State.Pursuing)
            OnPlayerDetected(fieldOfView.PlayerTransform.position);
    }

    /// <summary>
    /// Сбрасывает флаг isHiding, когда игрок вышел за радиус разрыва.
    /// </summary>
    private void TryResetHiding()
    {
        if (playerTransform != null)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > CHASE_BREAK_RADIUS)
                isHiding = false;
        }
        else
        {
            isHiding = false;
        }
    }

    /// <summary>
    /// Реагирует на обнаружение игрока в зависимости от текущего состояния.
    /// </summary>
    private void OnPlayerDetected(Vector3 playerPosition)
    {
        switch (currentState)
        {
            case State.Walking:
            case State.Returning:
                EnterAlertedState(playerPosition);
                break;

            case State.Alerted:
            case State.Searching:
                StartTurnAndDie(playerPosition);
                break;

            case State.Pursuing:
                isLookingAround = false;
                lastSeenPlayerPosition = playerPosition;
                animator?.SetBool("IsRunning", true);
                animator?.SetBool("IsWalking", false);
                break;
        }
    }

    private void EnterAlertedState(Vector3 playerPosition)
    {
        currentState = State.Alerted;
        lastSeenPlayerPosition = playerPosition;
        stateTimer = 0f;
        isHiding = true;
    }
    #endregion

    // ─── Обработчики состояний ────────────────────────────────────────────────
    #region State handlers

    /// <summary>
    /// Патруль между точками маршрута.
    /// </summary>
    private void HandleWalking()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[currentPointIndex];
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        // Кэшируем направление только при значимом изменении
        if (directionToTarget.magnitude > 0.1f &&
            (!Mathf.Approximately(directionToTarget.magnitude, cachedDirectionToTarget.magnitude)
             || !IsValidDirection()))
        {
            cachedDirectionToTarget = directionToTarget.normalized;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, SPEED_WALK * Time.deltaTime);

        if (IsValidDirection())
        {
            Quaternion targetRotation = Quaternion.LookRotation(cachedDirectionToTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SPEED_ROTATE * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) <= WAYPOINT_ARRIVE_DIST)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            stateTimer = 0f;
        }
    }

    /// <summary>
    /// Тревога → преследование / поиск.
    /// </summary>
    private void HandleAlertedOrSearching()
    {
        stateTimer += Time.deltaTime;

        if (currentState == State.Alerted)
        {
            FaceLastSeenPosition();

            if (stateTimer >= ALERT_DELAY)
            {
                currentState = State.Pursuing;
                stateTimer = 0f;
                pursuitLostTimer = 0f;
            }
        }
        else // State.Searching
        {
            if (fieldOfView.CheckForDetection() != FieldOfView.DetectionType.None)
            {
                StartTurnAndDie(lastSeenPlayerPosition);
                return;
            }

            PerformSearchScan();

            if (stateTimer >= SEARCH_DURATION)
            {
                currentState = State.Walking;
                stateTimer = 0f;
                isHiding = false;
            }
        }
    }

    /// <summary>
    /// Преследование: бежит за игроком, пока не потеряет его.
    /// </summary>
    private void HandlePursuing()
    {
        TryDropBreadcrumb();

        if (playerTransform == null)
            return;

        FieldOfView.DetectionType detection = fieldOfView.CheckForDetection();
        bool playerVisible = detection != FieldOfView.DetectionType.None
                          && detection != FieldOfView.DetectionType.InstantDeath;

        if (playerVisible)
        {
            ChasePlayer();
            return;
        }

        if (isLookingAround)
        {
            HandleLookingAround();
            TryFinishLookAround();
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer < MELEE_KILL_RADIUS)
        {
            StartTurnAndDie(playerTransform.position);
            return;
        }

        if (distToPlayer > CHASE_BREAK_RADIUS)
        {
            StartLookAround();
            return;
        }

        if (IsPlayerBehindWall(distToPlayer))
        {
            pursuitLostTimer += Time.deltaTime;
            if (pursuitLostTimer >= PURSUIT_LOST_TIMEOUT)
            {
                SwitchToReturning();
                return;
            }
        }
        else
            pursuitLostTimer = 0f;

        isLookingAround = false;
        lastSeenPlayerPosition = playerTransform.position;
        MoveWithSteering(lastSeenPlayerPosition, SPEED_RUN);
    }

    /// <summary>
    /// Возврат к маршруту по «хлебным крошкам».
    /// </summary>
    private void HandleReturning()
    {
        if (returnPath.Count == 0)
        {
            currentState = State.Walking;
            breadcrumbs.Clear();
            waypointStuckTimer = 0f;
            return;
        }

        Vector3 target = returnPath[0];
        float dist = Vector3.Distance(transform.position, target);

        UpdateStuckTimer();

        if (waypointStuckTimer >= WAYPOINT_STUCK_TIME && returnPath.Count > 1)
        {
            returnPath.RemoveAt(0);
            waypointStuckTimer = 0f;
            return;
        }

        MoveWithSteering(target, SPEED_WALK);

        if (dist < RETURN_ARRIVE_DIST)
        {
            returnPath.RemoveAt(0);
            waypointStuckTimer = 0f;
        }
    }
    #endregion

    // ─── Вспомогательные методы состояний ────────────────────────────────────
    #region State helpers

    private void ChasePlayer()
    {
        isLookingAround = false;
        lastSeenPlayerPosition = playerTransform.position;
        animator?.SetBool("IsRunning", true);
        animator?.SetBool("IsWalking", false);
        MoveWithSteering(lastSeenPlayerPosition, SPEED_RUN);
    }

    private void StartLookAround()
    {
        if (isLookingAround) return;
        isLookingAround = true;
        lookTimer = 0f;
        lookStartRot = transform.rotation;
        isHiding = false;
        smoothMoveDir = Vector3.zero;
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsWalking", false);
        HandleLookingAround();
        TryFinishLookAround();
    }

    private void TryFinishLookAround()
    {
        if (lookTimer >= LOOK_PHASE_DURATION * 4f)
        {
            isLookingAround = false;
            SwitchToReturning();
        }
    }

    private void SwitchToReturning()
    {
        currentState = State.Returning;
        pursuitLostTimer = 0f;
        BuildReturnPath();
    }

    /// <summary>
    /// Осмотр по сторонам поэтапным поворотом.
    /// </summary>
    private void HandleLookingAround()
    {
        lookTimer += Time.deltaTime;

        int currentPhase = Mathf.FloorToInt(lookTimer / LOOK_PHASE_DURATION);
        float baseY = lookStartRot.eulerAngles.y;

        Quaternion targetRot = currentPhase switch
        {
            0 => lookStartRot,
            1 => Quaternion.Euler(0, baseY - 45f, 0),
            2 => lookStartRot,
            3 => Quaternion.Euler(0, baseY + 45f, 0),
            _ => lookStartRot
        };

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, SCAN_ROTATION_SPEED * Time.deltaTime);
    }

    /// <summary>
    /// Синусоидальный поиск в районе последней позиции игрока.
    /// </summary>
    private void PerformSearchScan()
    {
        Vector3 baseDir = lastSeenPlayerPosition - transform.position;
        baseDir.y = 0f;

        if (baseDir.magnitude < 0.01f)
            baseDir = transform.forward;

        float angleOffset = LOOK_ANGLE * Mathf.Sin(Mathf.PI * 2 * stateTimer / SEARCH_DURATION);
        Quaternion baseRot = Quaternion.LookRotation(baseDir.normalized, Vector3.up);
        Quaternion searchRot = baseRot * Quaternion.Euler(0, angleOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, searchRot, SPEED_ROTATE * Time.deltaTime);
    }

    /// <summary>
    /// Плавно поворачивается к последней позиции игрока.
    /// </summary>
    private void FaceLastSeenPosition()
    {
        Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, SPEED_ROTATE * Time.deltaTime);
        }
    }

    /// <summary>
    /// Проверяет, перекрывает ли стена прямой луч к игроку.
    /// </summary>
    private bool IsPlayerBehindWall(float distToPlayer)
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        dirToPlayer.y = 0f;

        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

        if (Physics.Raycast(rayOrigin, dirToPlayer.normalized, out RaycastHit hit, distToPlayer, raycastMask, QueryTriggerInteraction.Ignore))
            return hit.collider.gameObject != playerTransform.gameObject;

        return false;
    }

    private void UpdateStuckTimer()
    {
        if (Vector3.Distance(transform.position, lastPosForWaypointStuck) < 0.05f)
            waypointStuckTimer += Time.deltaTime;
        else
            waypointStuckTimer = 0f;

        lastPosForWaypointStuck = transform.position;
    }
    #endregion

    // ─── Проигрыш игрока ────────────────────────────────────────────────────────
    #region Game Over
    private void StartTurnAndDie(Vector3 playerPosition)
    {
        lastSeenPlayerPosition = playerPosition;

        Vector3 dirToPlayer = playerPosition - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.magnitude > 0.1f)
        {
            targetRotationToPlayer = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
            turnStartTime = Time.time;
            isTurningToPlayer = true;
            StartCoroutine(TriggerGameOverWithDelay());
        }
        else
        {
            TriggerGameOver();
        }
    }

    private void UpdateTurnToPlayer()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationToPlayer, SPEED_ROTATE * Time.deltaTime);
        fieldOfView?.SetMaterial(fovRed);

        if (Time.time - turnStartTime >= GAMEOVER_TURN_TIME)
            isTurningToPlayer = false;
    }

    private IEnumerator TriggerGameOverWithDelay()
    {
        yield return new WaitForSeconds(TURN_TO_PLAYER_DELAY);
        TriggerGameOver();
    }

    private void TriggerGameOver() => gameOver.GameOverPanel();
    #endregion

    // ─── Движение ─────────────────────────────────────────────────────────────
    #region Movement

    /// <summary>
    /// Движение к цели с объездом препятствий: выбирает лучшее из 6 направлений.
    /// </summary>
    private void MoveWithSteering(Vector3 target, float speed)
    {
        Vector3 toTarget = target - transform.position;
        if (toTarget.y < 0) toTarget.y = 0f;

        if (toTarget.magnitude < STEERING_ARRIVE_DIST)
            return;

        Vector3 targetDir = toTarget.normalized;
        bool isClimbing = target.y > transform.position.y + 0.5f;

        Vector3 bestDir = PickBestDirection(targetDir, isClimbing);

        smoothMoveDir = Vector3.Slerp(smoothMoveDir, bestDir, DIR_SMOOTH_SPEED * Time.deltaTime);

        if (smoothMoveDir.magnitude > 0.1f)
        {
            ApplyMove(smoothMoveDir, speed, isClimbing);
            FaceDirection(smoothMoveDir);
        }
    }

    /// <summary>
    /// Выбирает направление с наилучшим балансом «вперёд + свободно».
    /// </summary>
    private Vector3 PickBestDirection(Vector3 targetDir, bool isClimbing)
    {
        if (isClimbing)
            return targetDir;

        Vector3[] candidates =
        {
            targetDir,
            Quaternion.Euler(0, -30, 0) * targetDir,
            Quaternion.Euler(0,  30, 0) * targetDir,
            Quaternion.Euler(0, -60, 0) * targetDir,
            Quaternion.Euler(0,  60, 0) * targetDir,
            -targetDir
        };

        Vector3 bestDir = targetDir;
        float bestScore = -999f;

        foreach (var dir in candidates)
        {
            float clearDist = GetClearDistance(dir);
            if (clearDist < 0.5f) continue;

            float score = Vector3.Dot(dir, targetDir) + clearDist * 0.4f;
            if (score > bestScore) { bestScore = score; bestDir = dir; }
        }

        return bestDir;
    }

    private void ApplyMove(Vector3 dir, float speed, bool isClimbing)
    {
        Vector3 moveStep = dir * speed * Time.deltaTime;

        if (!isClimbing)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, moveStep.magnitude + 0.1f, raycastMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject != gameObject)
                    moveStep = dir * Mathf.Max(0f, hit.distance - 0.15f);
            }
        }

        transform.position += moveStep;
        cachedDirectionToTarget = dir.normalized;
    }

    private void FaceDirection(Vector3 dir)
    {
        Vector3 lookDir = dir;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, SPEED_ROTATE * Time.deltaTime);
        }
    }

    private float GetClearDistance(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, CLEAR_PATH_CAST_DIST, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject) return 10f;
            return hit.distance;
        }
        return 10f;
    }
    #endregion

    // ─── Хлебные крошки (путь назад) ─────────────────────────────────────────
    #region Breadcrumbs & return path
    private void TryDropBreadcrumb()
    {
        if (Vector3.Distance(transform.position, lastBreadcrumbPos) < BREADCRUMB_DISTANCE)
            return;

        breadcrumbs.Add(transform.position);
        lastBreadcrumbPos = transform.position;

        if (breadcrumbs.Count > MAX_BREADCRUMBS)
            breadcrumbs.RemoveAt(0);
    }

    /// <summary>
    /// Строит оптимальный маршрут назад: срезает петли, выбирая самую дальнюю
    /// видимую крошку через SphereCast.
    /// </summary>
    private void BuildReturnPath()
    {
        returnPath.Clear();
        isHiding = false;

        // Список кандидатов: текущая позиция → крошки (в обратном порядке) → точка маршрута
        List<Vector3> candidates = new(breadcrumbs.Count + 2) { transform.position };

        for (int i = breadcrumbs.Count - 1; i >= 0; i--)
            candidates.Add(breadcrumbs[i]);

        if (patrolPoints != null && patrolPoints.Length > 0)
            candidates.Add(patrolPoints[currentPointIndex].position);

        // Жадный алгоритм: прыгаем через как можно больше крошек без стен
        int current = 0;
        while (current < candidates.Count - 1)
        {
            int farthestClear = current + 1;

            for (int i = candidates.Count - 1; i > current + 1; i--)
            {
                if (HasClearPath(candidates[current], candidates[i]))
                {
                    farthestClear = i;
                    break;
                }
            }

            returnPath.Add(candidates[farthestClear]);
            current = farthestClear;
        }
    }

    private bool HasClearPath(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.5f) return true;
        return !Physics.SphereCast(from + Vector3.up * 0.6f, 0.4f, dir.normalized, out _, dist, raycastMask);
    }
    #endregion

    // ─── Физика / корректировка позиции ──────────────────────────────────────
    #region Physics corrections

    /// <summary>
    /// Заставляет охранника плавно подстраиваться по высоте под поверхность земли
    /// </summary>
    private void FollowGround()
    {
        bool foundGround = Physics.Raycast(
            transform.position + Vector3.up * 2f,
            Vector3.down, out RaycastHit hit, 3f,
            raycastMask, QueryTriggerInteraction.Ignore);

        float targetY;

        if (foundGround && hit.collider.gameObject != gameObject)
        {
            if (hit.collider.CompareTag("Water"))
                targetY = hit.point.y - WATER_SUBMERGE_DEPTH; // "тонет" на нужную глубину
            else
                targetY = hit.point.y + GROUND_FOLLOW_HEIGHT;
        }
        else if (playerTransform != null)
            targetY = playerTransform.position.y + GROUND_FOLLOW_HEIGHT; // крайний случай, если рейкаст вообще ничего не нашёл
        else
            return;

        float smoothSpeed = (foundGround) ? GROUND_SMOOTH_SPEED : GROUND_SMOOTH_SPEED * 0.5f;
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, smoothSpeed * Time.deltaTime);
        transform.position = pos;
    }

    /// <summary>
    /// Расталкивает охранников, стоящих вплотную друг к другу (только X/Z).
    /// </summary>
    private void AvoidGuards()
    {
        foreach (var other in allGuards)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist >= GUARD_PUSH_RADIUS || dist <= GUARD_MIN_DIST) continue;

            Vector3 push = transform.position - other.transform.position;
            push.y = 0f;

            if (push.sqrMagnitude > 0.01f)
                transform.position += push.normalized * GUARD_PUSH_STEP;
        }
    }

    /// <summary>
    /// Выталкивает охранника из стены, если произошёл клиппинг.
    /// </summary>
    private void ResolveWallClipping()
    {
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

        foreach (var dir in directions)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, WALL_CHECK_DIST, raycastMask, QueryTriggerInteraction.Ignore))
            {
                bool hitPlayer = hit.collider.gameObject == playerTransform?.gameObject;
                bool hitSelf = hit.collider.gameObject == gameObject;

                if (!hitPlayer && !hitSelf)
                {
                    float pushAmount = WALL_CHECK_DIST - hit.distance + 0.01f;
                    transform.position -= dir.normalized * pushAmount;
                }
            }
        }
    }
    #endregion

    // ─── Анимация ─────────────────────────────────────────────────────────────
    #region Animator

    /// <summary>
    /// Обновляет анимацию
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator == null) return;

        float rawSpeed = (transform.position - lastAnimPos).magnitude / Mathf.Max(Time.deltaTime, 0.01f);
        lastAnimPos = transform.position;

        bool isMoving = rawSpeed > ANIM_SPEED_THRESHOLD;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);

        if (currentState == State.Pursuing && isMoving)
            animator.SetBool("IsRunning", true);
        else if (isMoving)
            animator.SetBool("IsWalking", true);
    }
    #endregion

    // ─── Публичный API ────────────────────────────────────────────────────────
    #region Public

    /// <summary>
    /// Возвращает true, пока охранник патрулирует (не тревога).
    /// </summary>
    public bool IsMoving() => currentState == State.Walking;

    private bool IsValidDirection() => cachedDirectionToTarget.magnitude > 0.1f;
    #endregion
}