using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

/// <summary>
/// скрипт патрулирования охранника - основная логика обнаружения
/// </summary>
public class GuardPatrol : MonoBehaviour
{
    /// <summary>Перечисление состояний поведения охранника</summary>
    public enum State { Walking, Alerted, Searching }


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

    private void Start()
    {
        gameOver = FindAnyObjectByType<GameOver>();

    }

    void Update()
    {
        PlayerCheck();
        if (currentState != State.Alerted && currentState != State.Searching)
        {
            Patrol();
        }
        else
        {
            HandleAlertedOrSearching();


        }

        if (fieldOfView != null)
        {
            fieldOfView.UpdateFOV(transform.position, transform.forward);
        }
        if (isTurningToPlayer)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationToPlayer, speedRotate * Time.deltaTime);
            float timeSinceStarted = Time.time - turnStartTime;

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
        if (currentState == State.Searching)
        {
            FieldOfView.DetectionType detection = fieldOfView.CheckForDetection();

            if (detection != FieldOfView.DetectionType.None)
            {
                StartTurnAndDie(lastSeenPlayerPosition);
                return; 
            }
        }

        stateTimer += Time.deltaTime;

        if (currentState == State.Alerted)
        {
            Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
            dirToPlayer.y = 0f;
            if (dirToPlayer != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
            }
            if (stateTimer >= alertTime)
            {
                currentState = State.Searching;
                stateTimer = 0f;
            }
        }
        else if (currentState == State.Searching)
        {
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
                isHiding = false;
            }
        }
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