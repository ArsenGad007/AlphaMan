using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

/// <summary>
/// скрипт патрулирования охранника - основная логика обнаружения
/// </summary>
public class GuardPatrol : MonoBehaviour
{
    /// <summary>Перечисление состояний поведения охранника</summary>
    public enum State { Walking, Alerted, Searching, Pursuing }

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
    /// <summary>
    [SerializeField] private Transform playerTransform;
    private Vector3 lastPosition;
    private float chaseBreakRadius = 15f;

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
                case State.Alerted:
                case State.Searching:
                    HandleAlertedOrSearching();
                    fieldOfView?.SetMaterial(Yellow);
                    break;
            }


            if (fieldOfView != null)
            {
                fieldOfView.UpdateFOV(transform.position, transform.forward);
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
    }
   
    /// <summary>
    /// Основная функция патрулирования, смена состояний
    /// </summary>
    private void Patrol()
    {
        switch (currentState)
        {
            case State.Walking:
                HandleWalking();
                break;
            /*case State.LookingAround:
                HandleLookingAround();
                break;*/
            case State.Pursuing: HandlePursuing(); break; 
        }
    }


    /// <summary>
    /// Логика перемещения между точками патруля
    /// </summary>
    private void HandleWalking()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[currentPointIndex];
        Vector3 directionToTarget = target.position - transform.position;

        if (directionToTarget.magnitude > 0.1f &&
            (!Mathf.Approximately(directionToTarget.magnitude, cachedDirectionToTarget.magnitude) || !isValidDirection()))
        {
            directionToTarget.y = 0f;

            if (directionToTarget.magnitude > 0.1f)
            {
                cachedDirectionToTarget = directionToTarget.normalized;
            }
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speedMove * Time.deltaTime
        );

        if (isValidDirection() && cachedDirectionToTarget.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cachedDirectionToTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                speedRotate * Time.deltaTime
            );
        }
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= 0.1f)
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

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distToPlayer > chaseBreakRadius)
        {
            currentState = State.Searching;
            stateTimer = 0f;
            return;
        }
        lastSeenPlayerPosition = playerTransform.position;
        MoveWithSteering(lastSeenPlayerPosition);
    }

    /// <summary>
    /// Движение к цели с плавным обходом стен
    /// </summary>
    /// <param name="target"></param>
    private void MoveWithSteering(Vector3 target)
    {
        Vector3 dirToTarget = target - transform.position;
        dirToTarget.y = 0f;
        if (dirToTarget.magnitude < 0.3f)
        {
            cachedDirectionToTarget = dirToTarget.normalized;
            return;
        }

        Vector3 forwardDir = dirToTarget.normalized;
        float checkDist = 1.5f;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f + forwardDir * 0.2f;
        bool blockedFront = IsPathBlocked(rayOrigin, forwardDir, checkDist);
        bool blockedLeft = IsPathBlocked(rayOrigin, Quaternion.Euler(0, -45, 0) * forwardDir, checkDist);
        bool blockedRight = IsPathBlocked(rayOrigin, Quaternion.Euler(0, 45, 0) * forwardDir, checkDist);
        Vector3 moveDir = forwardDir;
        if (blockedFront)
        {
            if (!blockedLeft) moveDir = Quaternion.Euler(0, -50, 0) * forwardDir;
            else if (!blockedRight) moveDir = Quaternion.Euler(0, 50, 0) * forwardDir;
           
        }
        else if (blockedLeft) moveDir = Quaternion.Euler(0, 25, 0) * forwardDir;
        else if (blockedRight) moveDir = Quaternion.Euler(0, -25, 0) * forwardDir;

        if (moveDir.magnitude > 0.1f)
        {
            transform.position += moveDir * speedMove * Time.deltaTime;
            cachedDirectionToTarget = moveDir.normalized;

            Quaternion targetRot = Quaternion.LookRotation(cachedDirectionToTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
        }
    }

    private bool IsPathBlocked(Vector3 origin, Vector3 dir, float dist)
    {
        int obstacleMask = LayerMask.GetMask("Obstacles");
        if (obstacleMask == 0) obstacleMask = ~0;
        return Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask) && !hit.collider.CompareTag("Door");
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
}