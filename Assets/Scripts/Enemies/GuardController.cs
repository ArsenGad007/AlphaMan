using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(GuardFieldOfView), typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    /// <summary>
    /// Состояния охранника
    /// </summary>
    public enum State { Walking, Hunt, Searching, Returning, Stop }

    [Header("Скорость")]
    [SerializeField][Min(0)] private float speedWalkMove = 2.0f;
    [SerializeField][Min(0)] private float speedRunMove = 4.5f;

    [Header("Радиусы обнаружения")]
    [Tooltip("Радиус зоны проигрыша (игрок проигрывает, если находится в этом радиусе и в зоне поля зрения)")]  
    [SerializeField] private float gameOverRadius = 1.3f;

    [Tooltip("Радиус зоны обнаружения, если игрок ходит (или стоит)")]
    [SerializeField] private float walkDetectionRadius = 1.5f;

    [Tooltip("Радиус зоны обнаружения, если игрок бежит")]
    [SerializeField] private float runDetectionRadius = 3f;

    [Header("Настройки NavMeshAgent")]
    [Tooltip("Дистанция, при которой NavMeshAgent считается дошедшим до точки патрулирования (way point)")]
    [SerializeField][Min(0)] private float pointStoppingDistance = 0.1f;

    [Tooltip("Дистанция, при которой NavMeshAgent считается дошедшим до игрока")]
    [SerializeField][Min(0)] private float playerStoppingDistance = 1.5f;

    [Header("Настройки преследования")]
    [Tooltip("Кол-во секунд, в течении которых знаем позицию игрока после его потери")]
    [SerializeField][Range(0, 5)] private float lostSightTimeout = 1f;

    [Tooltip("Чем больше, тем реже обновляет путь при преследовании")]
    [SerializeField][Range(0, 0.1f)] private float huntPathUpdateFactor = 0.03f; 

    [SerializeField] private Transform player;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private List<Vector3> wayPoints;
    
    private GuardFieldOfView guardFOV;
    private NavMeshAgent agent;

    private int currentNumPoint;            // Текущий номер точки (для wayPoints)
    private float lastTimeSeenPlayer = 0f;  // Последнее время, когда видели игрока
    private float nextPathUpdateTime = 0f;  // Время обновления следующего пути
    private bool pointReached = false;      
    private Vector3 lastPosPlayer;          // Последняя позиция игрока
    private int layerMask;                  // Маска слоев для Raycast

    /// <summary>
    /// Текущее состояние охранника
    /// </summary>
    public State currentState { get; private set; } = State.Walking;

    private void Awake()
    {
        if (!player)
            Debug.LogError("К объекту охраны не привязан Player");

        if (wayPoints != null && wayPoints.Count != 0)
            currentNumPoint = 0;

        layerMask = ~LayerMask.GetMask("Guard");        // Убираем слой охранников, чтобы не считали друг друга препятствием
        agent = GetComponent<NavMeshAgent>();
        guardFOV = GetComponent<GuardFieldOfView>();
    }

    private void Start()
    {
        EnterWalkingState();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Walking: Walking();  break;
            case State.Hunt:    Hunt();     break;
            case State.Searching:
                if (guardFOV.IsPlayerInFOV() || CanDetectPlayer())
                {
                    StopAllCoroutines();
                    StopSearching();
                    EnterRunningState();
                    currentState = State.Hunt;
                }               
                break;
        }

        guardFOV.UpdateFOV(transform.position, transform.forward);
    }

    /// <summary>
    /// Проверка что охранник дошел до нужной позиции
    /// </summary>
    private bool IsAgentAtDestination()
    {
        if (agent.pathPending || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f;
    }

    /// <summary>
    /// Патрулирование по точкам
    /// </summary>
    private void Walking()
    {
        if (IsAgentAtDestination())
        {
            if (!pointReached)  // Защита если точки находятся близко (чтобы не вызывать все время SetDestination)
            {
                currentNumPoint++;

                if(currentNumPoint >= wayPoints.Count)
                    currentNumPoint = 0;

                agent.SetDestination(wayPoints[currentNumPoint]);
                pointReached = true;
            }
        }
        else
            pointReached = false;

        if (guardFOV.IsPlayerInFOV() || CanDetectPlayer())
        {
            EnterRunningState();
            currentState = State.Hunt;
        }
    }

    /// <summary>
    /// Преследование игрока
    /// </summary>
    private void Hunt()
    {
        float distance;
        if (CheckObstacleToPlayerDir(out distance))
        {
            lastTimeSeenPlayer = Time.time;
            lastPosPlayer = player.position;
        }
        else if (Time.time - lastTimeSeenPlayer < lostSightTimeout)   // Идем к позиции игрока после потери в течении lostSightTimeout
            lastPosPlayer = player.position;

        if (Time.time >= nextPathUpdateTime)
        {
            agent.SetDestination(lastPosPlayer);
            nextPathUpdateTime = Time.time + distance * huntPathUpdateFactor;  
        }
        
        if (guardFOV.IsPlayerInFOV() && distance <= gameOverRadius)
        {
            guardFOV.SetRedMaterial();
            currentState = State.Stop;
            agent.isStopped = true;
            GameOver.Instance?.GameOverPanel();
        }
        else if (IsAgentAtDestination())
            StartSearching();      
    }

    /// <summary>
    /// Начать поиск игрока
    /// </summary>
    private void StartSearching()
    {
        currentState = State.Searching;
        agent.isStopped = true;
        agent.updateRotation = false;
        StartCoroutine(LookAround());
    }

    //////////////////////////////////////// Вспомогательные методы //////////////////////////////////////// 

    /// <summary>
    /// Остановить поиск игрока
    /// </summary>
    private void StopSearching()
    {
        agent.isStopped = false;
        agent.updateRotation = true;
    }
    /// <summary>
    /// Можно ли обнаружить игрока
    /// </summary>
    /// <returns></returns>
    private bool CanDetectPlayer()
    {
        if (!CheckObstacleToPlayerDir(out float distance))
            return false;

        float detectionRadius = gameInput.IsRunning()
            ? runDetectionRadius
            : walkDetectionRadius;

        return distance <= detectionRadius;
    }

    /// <summary>
    /// Вычисляет есть ли препятствие между охранником и игроком
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private bool CheckObstacleToPlayerDir(out float distance)
    {
        Vector3 eye_offset = Vector3.up * 1.5f;
        Vector3 from = transform.position + eye_offset;
        Vector3 to = player.position + eye_offset;
        Vector3 dir = to - from;

        distance = dir.magnitude;     

        RaycastHit hit_info;
        if (Physics.Raycast(from, dir.normalized, out hit_info, distance + 0.1f, layerMask))  // 0.1f в distance берем с запасом
            return hit_info.transform == player.transform;

        return false;            
    }

    /// <summary>
    /// Настройки для ходьбы
    /// </summary>
    private void EnterWalkingState()
    {
        guardFOV.SetGreenMaterial();

        agent.speed = speedWalkMove;
        agent.stoppingDistance = pointStoppingDistance;

        agent.SetDestination(wayPoints[currentNumPoint]);
    }

    /// <summary>
    /// Настройки для бега
    /// </summary>
    private void EnterRunningState()
    {
        guardFOV.SetYellowMaterial();

        agent.speed = speedRunMove;
        agent.stoppingDistance = playerStoppingDistance;

        lastPosPlayer = player.position;
        agent.SetDestination(lastPosPlayer);
    }

    /// <summary>
    /// Смотреть по сторонам
    /// </summary>
    private IEnumerator LookAround()
    {       
        yield return RotateBy(70);                                              // Поворот вправо на 90°       
        yield return new WaitForSeconds(0.5f);                                  // Пауза
                                                                               
        yield return RotateBy(-140);                                                      
        yield return new WaitForSeconds(0.5f);           
        
        yield return RotateBy(70);

        StopSearching();
        EnterWalkingState();
        currentState = State.Walking;                                           
    }

    /// <summary>
    /// Поворот в нужный угол
    /// </summary>
    /// <param name="degrees"></param>
    /// <param name="duration"></param>
    private IEnumerator RotateBy(float degrees, float duration = 1.5f)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, degrees, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);        
            float smoothT = t * t * (3f - 2f * t);      // Smoothstep даёт плавный разгон и угасание (ease-in-out)

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    /// <summary>
    /// Рисует точки пути охранника
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (wayPoints == null || wayPoints.Count == 0)
            return;

        Gizmos.color = Color.red;

        foreach (var point in wayPoints)
            Gizmos.DrawSphere(point, 0.2f);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < wayPoints.Count - 1; i++)
            Gizmos.DrawLine(wayPoints[i], wayPoints[i + 1]);

        // Замыкание маршрута
        if (wayPoints.Count > 1)
            Gizmos.DrawLine(wayPoints.Last(), wayPoints.First());
    }
}
