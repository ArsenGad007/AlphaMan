using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

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
    [SerializeField] private float pursueDuration = 5f;
    /// задержка перед поиском
    private float alertTime = 0.5f;
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
    private Vector3 lastPosition;
    private float chaseBreakRadius = 15f;

    private List<Vector3> breadcrumbs = new List<Vector3>();
    private List<Vector3> returnPath = new List<Vector3>();
    private Vector3 lastBreadcrumbPos;
    private const float BREADCRUMB_DISTANCE = 2f;
    private const int MAX_BREADCRUMBS = 50;
    private float fixedGroundY;
    private Vector3 lastAnimPos;
    private float speedRun = 3.5f;


    void Awake()
    {
        Green = Resources.Load<Material>("FOV_mat/FOV_Walking");
        Yellow = Resources.Load<Material>("FOV_mat/FOV_Alert");
        Red = Resources.Load<Material>("FOV_mat/FOV_Danger");
    }
    private void Start()
    {
        gameOver = FindAnyObjectByType<GameOver>();
        lastPosition = transform.position;
        lastBreadcrumbPos = transform.position;
        fixedGroundY = patrolPoints[0].position.y;

        Vector3 startPos = transform.position;
        startPos.y = fixedGroundY;
        transform.position = startPos;
        lastAnimPos = transform.position;

    }

    void Update()
    {
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
        Vector3 correctedPos = transform.position;
        correctedPos.y = fixedGroundY;
        transform.position = correctedPos;
        if (fieldOfView != null)
            {
            fieldOfView.UpdateFOV(correctedPos, transform.forward);
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
    }
    private void UpdateAnimator()
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

        if (!IsPathBlocked(transform.position + Vector3.up * 0.5f, directionToTarget.normalized, 1.2f))
        {
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
        }
        else
        {
            MoveWithSteering(target.position, speedMove);
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

    /*private void HandleLookingAround()
    {
        stateTimer += Time.deltaTime;
        Transform target = patrolPoints[currentPointIndex];
        Vector3 baseForward = target.forward;
        baseForward.y = 0f;
        if (baseForward == Vector3.zero) baseForward = Vector3.forward;

        float angleOffset = lookAngle * Mathf.Sin(Mathf.PI * 2 * stateTimer / lookAroundDuration);
        Quaternion baseRotation = Quaternion.LookRotation(baseForward, Vector3.up);
        Quaternion lookRotation = baseRotation * Quaternion.Euler(0, angleOffset, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, speedRotate * Time.deltaTime);

        if (stateTimer >= lookAroundDuration)
        {
            currentState = State.Walking; 
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }*/


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
        }

        if (detection == FieldOfView.DetectionType.None)
        {
            isHiding = false;
            return;
        }

        if (currentState == State.Searching)
        {
            StartTurnAndDie(lastSeenPlayerPosition);
            return;
        }
        if (isHiding)
            return;

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


    ///Преследование, идёт за игроком, пока тот не уйдёт за радиус разрыва</summary>
    private void HandlePursuing()
    {
        if (playerTransform == null) return;
        TryDropBreadcrumb();

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distToPlayer > chaseBreakRadius)
        {
            currentState = State.Returning;
            
            stateTimer = 0f;BuildReturnPath();
            return;
        }
        lastSeenPlayerPosition = playerTransform.position;
        MoveWithSteering(lastSeenPlayerPosition, speedRun);
    }
    private Vector3 smoothMoveDir;
    private const float DIR_SMOOTH_SPEED = 6f;

    /// Движение к цели с плавным обходом стен
    private void MoveWithSteering(Vector3 target, float currentSpeed)
    {
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude < 0.4f) return;

        Vector3 targetDir = toTarget.normalized;

        Vector3[] dirs = {
        targetDir,
        Quaternion.Euler(0, -30, 0) * targetDir,
        Quaternion.Euler(0, 30, 0) * targetDir,
        Quaternion.Euler(0, -60, 0) * targetDir,
        Quaternion.Euler(0, 60, 0) * targetDir,
        -targetDir
    };

        Vector3 bestDir = targetDir;
        float bestScore = -999f;

        for (int i = 0; i < dirs.Length; i++)
        {
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
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            int mask = LayerMask.GetMask("Obstacles");
            if (mask != 0 && Physics.Raycast(rayOrigin, smoothMoveDir, out RaycastHit hit, stepDist + 0.1f, mask, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObj = hit.collider.gameObject;
                bool isDoor = hitObj.CompareTag("Door") ||
                              (hitObj.transform.parent != null && hitObj.transform.parent.CompareTag("Door")) ||
                              hit.collider.isTrigger;

                if (!isDoor)
                {
                    float safeDist = Mathf.Max(0f, hit.distance - 0.15f);
                    moveStep = smoothMoveDir * safeDist;
                }
            }

            transform.position += moveStep;

            cachedDirectionToTarget = smoothMoveDir.normalized;
            Quaternion targetRot = Quaternion.LookRotation(cachedDirectionToTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
            Vector3 e = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, e.y, 0);
        }
    }

    /// <summary>Возвращает дистанцию до ближайшего препятствия в направлении</summary>
    private float GetClearDistance(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        int mask = LayerMask.GetMask("Obstacles");
        if (mask == 0) return 10f;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, 2.5f, mask, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("Door") ||
                (hitObj.transform.parent != null && hitObj.transform.parent.CompareTag("Door")) ||
                hit.collider.isTrigger)
                return 10f;
            return hit.distance;
        }
        return 10f;
    }
    private bool IsPathBlocked(Vector3 origin, Vector3 dir, float dist)
    {
        int obstacleMask = LayerMask.GetMask("Obstacles");
        if (obstacleMask == 0) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("Door") ||
                (hitObj.transform.parent != null && hitObj.transform.parent.CompareTag("Door")) ||
                hit.collider.isTrigger)
                return false;
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
        return !IsPathBlocked(from + Vector3.up * 0.6f, dir.normalized, dist);
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
}