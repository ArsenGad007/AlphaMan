using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GuardController : MonoBehaviour
{
    /// <summary>
    /// Состояния охранника
    /// </summary>
    public enum State { Walking, Chase, Searching, Returning, Stop }

    [SerializeField][Min(0)] private float speedWalkMove = 2.0f;
    [SerializeField][Min(0)] private float speedRunMove = 4.5f;

    [Tooltip("Дистанция, при которой NavMeshAgent считается дошедшим до точки патрулирования (way point)")]
    [SerializeField][Min(0)] private float pointStoppingDistance = 0.1f;

    [Tooltip("Дистанция, при которой NavMeshAgent считается дошедшим до игрока")]
    [SerializeField][Min(0)] private float playerStoppingDistance = 1.5f;

    [Tooltip("Сколько секунд знаем позицию игрока после его потери")]
    [SerializeField][Range(0, 5)] private float lostSightTimeout = 1f; 

    [SerializeField] private Transform player;
    [SerializeField] private List<Vector3> wayPoints;
    
    private GuardFieldOfView guardFOV;
    private NavMeshAgent agent;

    private int currentNumPoint;
    private float lastSeenTime = 0f;
    private bool pointReached = false;
    private Vector3 lastSeenPlayer;

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
            case State.Walking:     Walking();      break;
            case State.Chase:       Chase();        break;
            case State.Searching:
                if (guardFOV.IsPlayerInFOV())
                {
                    StopAllCoroutines();
                    StopSearching();
                    EnterRunningState();
                    currentState = State.Chase;
                }               
                break;
        }

        guardFOV?.UpdateFOV(transform.position, transform.forward);
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
            if (!pointReached)  // Защита если точки находятся близко
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

        if (guardFOV.IsPlayerInFOV())
        {
            EnterRunningState();
            currentState = State.Chase;
        }
    }

    /// <summary>
    /// Преследование игрока
    /// </summary>
    private void Chase()
    {
        Vector3 eye_offset = Vector3.up * 1.5f;    
        Vector3 from = transform.position + eye_offset;
        Vector3 to = player.position + eye_offset;
        Vector3 dir = to - from;

        float distance = dir.magnitude + 0.1f;          // 0.1f берем с запасом
        int layer_mask = ~LayerMask.GetMask("Guard");   // Убираем слой охранников, чтобы не считали друг друга препятствием

        RaycastHit hit_info;
        bool check_hit = Physics.Raycast(from, dir.normalized, out hit_info, distance, layer_mask);
        if (check_hit && hit_info.transform == player.transform)
        {
            lastSeenTime = Time.time;
            lastSeenPlayer = player.position;
        }
        else if (Time.time - lastSeenTime < lostSightTimeout)   // Идем к позиции игрока после потери в течении lostSightTimeout
            lastSeenPlayer = player.position;

        agent.SetDestination(lastSeenPlayer);

        if (guardFOV.IsPlayerInFOV() && guardFOV.IsPersonInInstantRange(player))
        {
            guardFOV.SetRedMaterial();
            currentState = State.Stop;
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

    /// <summary>
    /// Остановить поиск игрока
    /// </summary>
    private void StopSearching()
    {
        agent.isStopped = false;
        agent.updateRotation = true;   
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

        lastSeenPlayer = player.position;
        agent.SetDestination(lastSeenPlayer);
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
