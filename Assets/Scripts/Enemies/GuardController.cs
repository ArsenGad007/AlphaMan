using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

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
            case State.Walking: Walking(); break;
            case State.Chase: Chase(); break;
        }

        guardFOV?.UpdateFOV(transform.position, transform.forward);
    }

    private bool IsAgentAtDestination()
    {
        if (agent.pathPending || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance
               && agent.velocity.sqrMagnitude < 0.01f;
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
        float distance = Vector3.Distance(transform.position, player.position);

        //if (distance > 1.5f)
        //    transform.position = Vector3.MoveTowards(transform.position, player.position, 3f * Time.deltaTime);

        agent.SetDestination(player.position);

        if (IsAgentAtDestination())
        {
            guardFOV.SetRedMaterial();
            currentState = State.Stop;
            GameOver.Instance?.GameOverPanel();
        }
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
