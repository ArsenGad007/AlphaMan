using UnityEngine;

public class GuardScript : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 5.0f;
    private int currentPointIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            //Debug.Log("нет точек патрулирования");
            return;
        }
        
        Transform target = patrolPoints[currentPointIndex];

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {     
            transform.rotation = Quaternion.LookRotation(dir);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            //Debug.Log("дошел до точки" + currentPointIndex);
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
                // Debug.Log("возвращается");
            }
        }
    }
}
