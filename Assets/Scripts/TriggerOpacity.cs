using UnityEngine;

public class TriggerOpacity : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Enter: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Exit: {other.name}");
    }
}
