using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEngine.UI.Image;

public class GuardController : MonoBehaviour
{
    /// <summary>
    /// Состояния охранника
    /// </summary>
    public enum State { Walking, Chase, Searching, Returning, Stop }

    [SerializeField][Min(0)] private float speedWalkMove = 2.0f;
    [SerializeField][Min(0)] private float speedRunMove = 4.6f;

    [SerializeField][Min(0)] private float pointStoppingDistance = 0.1f;
    [SerializeField][Min(0)] private float playerStoppingDistance = 1.5f;   

    [SerializeField] private Transform player;
    [SerializeField] private List<Vector3> wayPoints;

    private GuardFieldOfView guardFOV;
    private NavMeshAgent agent;

    private int currentNumPoint;
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
        guardFOV.SetGreenMaterial();

        agent.speed = speedWalkMove;
        agent.stoppingDistance = pointStoppingDistance;
        agent.SetDestination(wayPoints[currentNumPoint]);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Walking:     Walking();      break;
            case State.Chase:       Chase();        break;
        }

        guardFOV?.UpdateFOV(transform.position, transform.forward);
    }

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
            lastSeenPlayer = player.position;
            guardFOV.SetYellowMaterial();
            currentState = State.Chase;

            agent.speed = speedRunMove;
            agent.stoppingDistance = playerStoppingDistance;
        }
    }

    /// <summary>
    /// Преследование игрока
    /// </summary>
    private void Chase()
    {
        float distance = Vector3.Distance(transform.position, player.position) + 1f;    // +1f берем с запасом

        Vector3 to_player = player.transform.position - transform.position;
        bool check_obstacle = Physics.Raycast(transform.position, to_player, distance);
        if (!check_obstacle)
            lastSeenPlayer = player.position;

        agent.SetDestination(lastSeenPlayer);

        if (guardFOV.IsPlayerInFOV() && guardFOV.IsPersonInInstantRange(player))
        {
            guardFOV.SetRedMaterial();
            currentState = State.Stop;
            GameOver.Instance?.GameOverPanel();
        }
        else if (IsAgentAtDestination())
        {
            currentState = State.Searching;
            Searching();
        }         
    }

    private void Searching() => StartCoroutine(LookAround());

    private IEnumerator LookAround()
    {
        agent.isStopped = true;
        agent.updateRotation = false;

        yield return RotateBy(70);                                              // Поворот вправо на 90°       
        yield return new WaitForSeconds(0.5f);                                  // Пауза
                                                                                // 
        yield return RotateBy(-140);                                                      
        yield return new WaitForSeconds(0.5f);           
        
        yield return RotateBy(70);                                              

        agent.updateRotation = true;
        agent.isStopped = false;

        agent.speed = speedWalkMove;
        agent.stoppingDistance = pointStoppingDistance;

        agent.SetDestination(wayPoints[currentNumPoint]);

        guardFOV.SetGreenMaterial();
        currentState = State.Walking;                                           // Возвращаемся в исходное положение
    }

    private IEnumerator RotateBy(float degrees, float duration = 1.5f)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, degrees, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothstep даёт плавный разгон и угасание (ease-in-out)
            float smoothT = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            yield return null;
        }

        transform.rotation = targetRotation;
    }


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
