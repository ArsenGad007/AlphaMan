using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private string playerTag = "Player";

    private bool itemCollected = false;
    private bool flagInteract = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (gameInput.IsInteract() && flagInteract && !itemCollected)
            {
                Sounds.Instance.PlayPickup();// звук взятия предмета
                targetObject.SetActive(false);
                itemCollected = true;

                flagInteract = false;
            }            
            else if (!gameInput.IsInteract())
                flagInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            flagInteract = false;
    }

    public bool ItemCollected() => itemCollected;
}
