using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

/// <summary>
/// скрипт патрулирования охранника - основная логика обнаружения
/// </summary>
public class GuardPatrol : MonoBehaviour
{
    /// <summary>Перечисление состояний поведения охранника</summary>
    public enum State { Walking, Alerted, Searching, Pursuing, Returning }

    [SerializeField] private Animator animator;
    // Настройки движения
    /// <summary>
    /// точки маршрута патруля
    /// </summary>
    [SerializeField] private Transform[] patrolPoints;
    /// <summary>
    /// скорость перемещения
    /// </summary>
    private float speedMove = 2f;
    /// <summary>
    /// скорость поворота
    /// </summary>
    private float speedRotate = 10f;

    /// <summary>
    /// поле зрения камеры
    /// </summary>
    [SerializeField] private FieldOfView fieldOfView;
    /// <summary>
    /// система проигрыша
    /// </summary>
    private GameOver gameOver;

    //Временные пар-тры
    /// <summary>
    /// длительность поиска
    /// </summary>
    private float searchDuration = 3f;
    /// задержка перед поиском
    private float alertTime = 0.2f;
    /// угол обзора
    private float lookAngle = 45f;


    // Переменные состояния
    /// <summary>
    /// Индекс текущей точки маршрута
    /// </summary>
    private int currentPointIndex = 0;
    /// <summary>
    /// Таймер текущего состояния
    /// </summary>
    private float stateTimer = 0f;
    /// <summary>
    /// Текущее поведение охранника
    /// </summary>
    private State currentState = State.Walking;

    // Информация об обнаружении
    /// <summary>
    /// Последняя известная позиция игрока
    /// </summary>
    private Vector3 lastSeenPlayerPosition;
    /// <summary>
    /// Флаг скрытия от охранника
    /// </summary>
    private bool isHiding = false;

    // Для плавного поворота к игроку перед смертью
    /// <summary>
    /// Поворот к игроку
    /// </summary>
    private Quaternion targetRotationToPlayer;
    /// <summary>
    /// Время начала поворота (для таймера)
    /// </summary>
    private float turnStartTime;
    /// <summary>
    /// В процессе ли поворот
    /// </summary>
    private bool isTurningToPlayer;
    /// <summary>
    /// // Кэшированное направление к цели для опт-ции
    /// </summary>
    private Vector3 cachedDirectionToTarget;
    /// <summary>
    /// цвета сетки
    /// </summary>
    private Material Green;
    private Material Yellow;
    private Material Red;
    ///
    [SerializeField] private Transform playerTransform;
    private float chaseBreakRadius = 12f;

    private List<Vector3> breadcrumbs = new List<Vector3>();
    private List<Vector3> returnPath = new List<Vector3>();
    private Vector3 lastBreadcrumbPos;
    private const float BREADCRUMB_DISTANCE = 2f;
    private const int MAX_BREADCRUMBS = 500;
    private float fixedGroundY;
    private Vector3 lastAnimPos;
    private float speedRun = 3.5f;

    private bool isLookingAround = false;
    private float lookTimer = 0f;
    private Quaternion lookStartRot;
    private const float LOOK_PHASE_DURATION = 0.7f;
    private const float SCAN_ROTATION_SPEED = 2.5f;
    private float groundFollowHeight = 0.1f;
    private float groundSmoothSpeed = 10f;
    private LayerMask raycastMask;
    private List<GuardPatrol> allGuards = new List<GuardPatrol>();
    private float pursuitLostTimeout = 4f;
    private float pursuitLostTimer = 0f;



    void Awake()
    {
        Green = Resources.Load<Material>("FOV_mat/FOV_Walking");
        Yellow = Resources.Load<Material>("FOV_mat/FOV_Alert");
        Red = Resources.Load<Material>("FOV_mat/FOV_Danger");
    }
    private void Start()
    {
        gameOver = FindAnyObjectByType<GameOver>();
        lastBreadcrumbPos = transform.position;
        //fixedGroundY = patrolPoints[0].position.y;
        //Vector3 startPos = transform.position;
        // startPos.y = fixedGroundY;
        //  transform.position = startPos;
        lastAnimPos = transform.position;
        raycastMask = ~LayerMask.GetMask("Guard");
        allGuards = new List<GuardPatrol>(Object.FindObjectsByType<GuardPatrol>(FindObjectsSortMode.None));

    }

    void Update()
    {

        if (fieldOfView != null)
        {
            fieldOfView.UpdateFOV(transform.position, transform.forward);
        }
        PlayerCheck();
        switch (currentState)
        {
            case State.Walking:
                HandleWalking();
                fieldOfView?.SetMaterial(Green);
                break;
            case State.Pursuing:
                HandlePursuing();
                fieldOfView?.SetMaterial(Yellow);
                break;
            case State.Returning:
                HandleReturning();
                fieldOfView?.SetMaterial(Yellow);
                break;
            case State.Alerted:
            case State.Searching:
                HandleAlertedOrSearching();
                fieldOfView?.SetMaterial(Yellow);
                break;
        }
        if (isTurningToPlayer)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationToPlayer, speedRotate * Time.deltaTime);
            float timeSinceStarted = Time.time - turnStartTime;
            fieldOfView?.SetMaterial(Red);

            if (timeSinceStarted >= 0.3f)
            {
                isTurningToPlayer = false;
            }

        }
        UpdateAnimator();
        RaycastHit hit;
        bool foundGround = Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 3f, raycastMask, QueryTriggerInteraction.Ignore);

        if (foundGround && !hit.collider.CompareTag("Water") && hit.collider.gameObject != gameObject)
        {
            float targetY = hit.point.y + groundFollowHeight;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetY, groundSmoothSpeed * Time.deltaTime);
            transform.position = pos;
        }
        else if (playerTransform != null)
        {
            float targetY = playerTransform.position.y + groundFollowHeight;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetY, groundSmoothSpeed * 0.5f * Time.deltaTime);
            transform.position = pos;
        }
        AvoidGuards();
        ResolveWallClipping();
    }
    /// <summary>расталкивание охранников (X/Z)</summary>
    private void AvoidGuards()
    {
        foreach (var other in allGuards)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < 1.0f && dist > 0.1f)
            {
                Vector3 push = transform.position - other.transform.position;
                push.y = 0f;
                if (push.sqrMagnitude > 0.01f)
                {
                    push.Normalize();
                    transform.position += push * 0.01f;
                }
            }
        }
    }
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            float rawSpeed = (transform.position - lastAnimPos).magnitude / Mathf.Max(Time.deltaTime, 0.01f);
            lastAnimPos = transform.position;
            bool isMoving = rawSpeed > 0.15f;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            if (currentState == State.Pursuing && isMoving)
            {
                animator.SetBool("IsRunning", true);
            }
            else if (isMoving)
            {
                animator.SetBool("IsWalking", true);
            }
        }
    }

    /// <summary>
    /// Логика перемещения между точками патруля
    /// </summary>
    private void HandleWalking()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        //TryDropBreadcrumb();
        Transform target = patrolPoints[currentPointIndex];
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;


        if (directionToTarget.magnitude > 0.1f &&
            (!Mathf.Approximately(directionToTarget.magnitude, cachedDirectionToTarget.magnitude) || !isValidDirection()))
        {
            cachedDirectionToTarget = directionToTarget.normalized;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speedMove * Time.deltaTime);

        if (isValidDirection() && cachedDirectionToTarget.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cachedDirectionToTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotate * Time.deltaTime);
        }


        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            stateTimer = 0f;
        }
    }

    /// <summary>
    /// Возвращает true если текущее направление имеет значение больше eps
    /// </summary>
    private bool isValidDirection() => cachedDirectionToTarget.magnitude > 0.1f;

    /// <summary>Плавный осмотр по сторонам при разрыве дистанции</summary>
    private void HandleLookingAround()
    {
        lookTimer += Time.deltaTime;

        int currentPhase = Mathf.FloorToInt(lookTimer / LOOK_PHASE_DURATION);
        float baseY = lookStartRot.eulerAngles.y;
        Quaternion targetRot = lookStartRot;
        switch (currentPhase)
        {
            case 0: targetRot = lookStartRot; break;
            case 1: targetRot = Quaternion.Euler(0, baseY - 45f, 0); break;
            case 2: targetRot = Quaternion.Euler(0, baseY, 0); break;
            case 3: targetRot = Quaternion.Euler(0, baseY + 45f, 0); break;
            case 4: targetRot = Quaternion.Euler(0, baseY, 0); break;
        }

        // Плавный поворот с фиксированной низкой скоростью
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, SCAN_ROTATION_SPEED * Time.deltaTime);
    }


    /// <summary>
    /// Проверяет видимость игрока через поле зрения камеры и запускает соотв действия.
    /// Обрабатывает мгновенную смерть в близкой дист, потерю видимости и состояние поиска
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
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist > chaseBreakRadius)
                {
                    isHiding = false;
                }
            }
            else
            {
                isHiding = false;
            }

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
    /// метод для запуска поворота к игроку и последующего проигрыша
    /// </summary>
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

    /// <summary>
    /// Корутин для задержки перед вызовом Game Over
    /// Дает время закончиться процессу поворота (0.05 секунды)
    /// </summary>
    private IEnumerator TriggerGameOverWithDelay()
    {
        yield return new WaitForSeconds(0.05f);
        TriggerGameOver();
    }

    /// <summary>
    /// Определяет, что делать, когда игрок встречен
    /// </summary>
    private void OnPlayerDetected(Vector3 playerPosition)
    {
        if (currentState == State.Walking)
        {
            EnterAlertedState(playerPosition);
        }
        else if (currentState == State.Alerted || currentState == State.Searching)
        {
            StartTurnAndDie(playerPosition);
        }
        else if (currentState == State.Returning)
        {
            EnterAlertedState(playerPosition);
        }
        else if (currentState == State.Pursuing)
        {
            isLookingAround = false;
            lastSeenPlayerPosition = playerPosition;
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsWalking", false);
        }
    }

    /// <summary>
    /// Вводит охранника в состояние Alerted при первом обнаружении игрока.
    /// Запоминает позицию и запускает таймер перехода в Searching.
    /// </summary>
    /// <param name="playerPosition"></param>
    private void EnterAlertedState(Vector3 playerPosition)
    {
        currentState = State.Alerted;
        lastSeenPlayerPosition = playerPosition;
        stateTimer = 0f;
        isHiding = true;

    }
    /// <summary>
    /// Обрабатывает состояния Alerted и Searching.
    /// </summary>
    private void HandleAlertedOrSearching()
    {
        stateTimer += Time.deltaTime;

        if (currentState == State.Alerted)
        {
            Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
            dirToPlayer.y = 0f;
            if (dirToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
            }
            if (stateTimer >= alertTime)
            {
                currentState = State.Pursuing;
                stateTimer = 0f;
                pursuitLostTimer = 0f;
            }
        }
        else if (currentState == State.Searching)
        {
            if (fieldOfView.CheckForDetection() != FieldOfView.DetectionType.None)
            {
                StartTurnAndDie(lastSeenPlayerPosition);
                return;
            }
            Vector3 baseDir = lastSeenPlayerPosition - transform.position;
            baseDir.y = 0f;
            if (baseDir.magnitude < 0.01f) baseDir = transform.forward;

            float angleOffset = lookAngle * Mathf.Sin(Mathf.PI * 2 * stateTimer / searchDuration);
            Quaternion baseRot = Quaternion.LookRotation(baseDir.normalized, Vector3.up);
            Quaternion searchRot = baseRot * Quaternion.Euler(0, angleOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, searchRot, speedRotate * Time.deltaTime);
            if (stateTimer >= searchDuration)
            {
                currentState = State.Walking;
                stateTimer = 0f;
                isHiding = false;
            }
        }
    }
    /// <summary>
    /// Преследование, идёт за игроком, пока тот не уйдёт за радиус разрыва
    /// </summary>
    private void HandlePursuing()
    {
        TryDropBreadcrumb();
        if (playerTransform == null) return;
        FieldOfView.DetectionType currentDetection = fieldOfView.CheckForDetection();
        if (currentDetection != FieldOfView.DetectionType.None && currentDetection != FieldOfView.DetectionType.InstantDeath)
        {
            isLookingAround = false;
            lastSeenPlayerPosition = playerTransform.position;

            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsWalking", false);
            }

            MoveWithSteering(lastSeenPlayerPosition, speedRun);
            return;
        }
        if (isLookingAround)
        {
            HandleLookingAround();
            if (lookTimer >= LOOK_PHASE_DURATION * 4f)
            {
                isLookingAround = false;
                currentState = State.Returning;
                BuildReturnPath();
            }
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer < 1.2f)
        {
            StartTurnAndDie(playerTransform.position);
            return;
        }
        if (distToPlayer > chaseBreakRadius)
        {
            if (!isLookingAround)
            {
                isLookingAround = true;
                lookTimer = 0f;
                lookStartRot = transform.rotation;
                isHiding = false;
                smoothMoveDir = Vector3.zero;
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsWalking", false);
            }
            HandleLookingAround();
            if (lookTimer >= LOOK_PHASE_DURATION * 4f)
            {
                isLookingAround = false;
                currentState = State.Returning;
                BuildReturnPath();
            }
            return;
        }
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        dirToPlayer.y = 0f;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

        if (Physics.Raycast(rayOrigin, dirToPlayer.normalized, out RaycastHit hit, distToPlayer, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != playerTransform?.gameObject)
            {
                pursuitLostTimer += Time.deltaTime;
                if (pursuitLostTimer >= pursuitLostTimeout)
                {
                    currentState = State.Returning;
                    BuildReturnPath();
                    pursuitLostTimer = 0f;
                    return;
                }
            }
            else
            {
                pursuitLostTimer = 0f;
            }
        }
        else
        {
            pursuitLostTimer = 0f;
        }
        isLookingAround = false;
        lastSeenPlayerPosition = playerTransform.position;
        MoveWithSteering(lastSeenPlayerPosition, speedRun);
    }
    private Vector3 smoothMoveDir;
    private const float DIR_SMOOTH_SPEED = 6f;

    /// <summary>
    /// Движение к цели с плавным обходом стен
    /// </summary>
    /// <param name="target"></param>
    /// <param name="currentSpeed"></param>
    private void MoveWithSteering(Vector3 target, float currentSpeed)
    {
        Vector3 toTarget = target - transform.position;
        if (toTarget.y < 0) toTarget.y = 0f;

        if (toTarget.magnitude < 0.4f) return;

        Vector3 targetDir = toTarget.normalized;
        bool isClimbing = target.y > transform.position.y + 0.5f;

        Vector3[] dirs;

        if (isClimbing)
        {
            dirs = new Vector3[] { targetDir };
        }
        else
        {

            dirs = new Vector3[] {
                targetDir,
                Quaternion.Euler(0, -30, 0) * targetDir,
                Quaternion.Euler(0, 30, 0) * targetDir,
                Quaternion.Euler(0, -60, 0) * targetDir,
                Quaternion.Euler(0, 60, 0) * targetDir,
                -targetDir
            };
        }

        Vector3 bestDir = targetDir;
        float bestScore = -999f;

        for (int i = 0; i < dirs.Length; i++)
        {
            if (isClimbing)
            {
                bestDir = dirs[i];
                bestScore = 100f;
                break;
            }

            float clearDist = GetClearDistance(dirs[i]);
            if (clearDist < 0.5f) continue;

            float alignment = Vector3.Dot(dirs[i], targetDir);
            float score = alignment + (clearDist * 0.4f);
            if (score > bestScore) { bestScore = score; bestDir = dirs[i]; }
        }

        smoothMoveDir = Vector3.Slerp(smoothMoveDir, bestDir, DIR_SMOOTH_SPEED * Time.deltaTime);

        if (smoothMoveDir.magnitude > 0.1f)
        {
            Vector3 moveStep = smoothMoveDir * currentSpeed * Time.deltaTime;
            float stepDist = moveStep.magnitude;
            if (!isClimbing)
            {
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
                if (Physics.Raycast(rayOrigin, smoothMoveDir, out RaycastHit hit, stepDist + 0.1f, raycastMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.gameObject != gameObject)
                    {
                        float safeDist = Mathf.Max(0f, hit.distance - 0.15f);
                        moveStep = smoothMoveDir * safeDist;
                    }
                }
            }

            transform.position += moveStep;

            cachedDirectionToTarget = smoothMoveDir.normalized;

            Vector3 lookDir = cachedDirectionToTarget;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
            }
        }
    }
    /// <summary>Возвращает дистанцию до ближайшего препятствия в направлении</summary>
    private float GetClearDistance(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, 2.5f, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject) return 10f;
            return hit.distance;
        }
        return 10f;
    }
    private bool IsPathBlocked(Vector3 origin, Vector3 dir, float dist)
    {
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject) return false;
            return true;
        }
        return false;
    }

    /// <summary>
    ///  Вызывает экран проигрыша
    /// </summary>
    private void TriggerGameOver()
    {
        gameOver.GameOverPanel();
    }
    /// <summary>
    ///  движется ли охранник (патрулируя)
    /// </summary>
    /// <returns></returns>
    public bool IsMoving()
    {

        return currentState == State.Walking;
    }

    private void TryDropBreadcrumb()
    {
        if (Vector3.Distance(transform.position, lastBreadcrumbPos) >= BREADCRUMB_DISTANCE)
        {
            breadcrumbs.Add(transform.position);
            lastBreadcrumbPos = transform.position;

            if (breadcrumbs.Count > MAX_BREADCRUMBS)
                breadcrumbs.RemoveAt(0);
        }
    }

    ///<summary>Строит возвратный маршрут срезает круги + выбирает оптимальную сторону обхода</summary>
    private void BuildReturnPath()
    {
        returnPath.Clear();
        isHiding = false;
        List<Vector3> candidates = new List<Vector3>(breadcrumbs.Count + 2);
        candidates.Add(transform.position);
        for (int i = breadcrumbs.Count - 1; i >= 0; i--)
            candidates.Add(breadcrumbs[i]);

        if (patrolPoints != null && patrolPoints.Length > 0)
            candidates.Add(patrolPoints[currentPointIndex].position);
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



    /// <summary>Проверяет прямую проходимость между двумя точками</summary>
    private bool HasClearPath(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.5f) return true;
        return !Physics.SphereCast(from + Vector3.up * 0.6f, 0.4f, dir.normalized, out _, dist, raycastMask);
    }
    private float waypointStuckTimer = 0f;
    private Vector3 lastPosForWaypointStuck;

    /// <summary>Возврат по "крошкам"</summary>
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

        if (Vector3.Distance(transform.position, lastPosForWaypointStuck) < 0.05f)
            waypointStuckTimer += Time.deltaTime;
        else
            waypointStuckTimer = 0f;
        lastPosForWaypointStuck = transform.position;
        if (waypointStuckTimer >= 2f && returnPath.Count > 1)
        {
            returnPath.RemoveAt(0);
            waypointStuckTimer = 0f;
            return;
        }
        MoveWithSteering(target, speedMove);
        if (dist < 0.8f)
        {
            returnPath.RemoveAt(0);
            waypointStuckTimer = 0f;
        }
    }
    /// <summary>
    /// Выталкивает охранника из стен, если произошел клиппинг-въезд в стену.
    /// </summary>
    private void ResolveWallClipping()
    {
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };
        float checkDistance = 0.6f;

        foreach (var dir in directions)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, checkDistance, raycastMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject != playerTransform?.gameObject &&
                    hit.collider.gameObject != gameObject)
                {
                    float pushAmount = checkDistance - hit.distance + 0.01f;
                    transform.position -= dir.normalized * pushAmount;
                }
            }
        }
    }
}