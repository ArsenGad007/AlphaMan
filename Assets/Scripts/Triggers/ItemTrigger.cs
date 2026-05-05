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
                SoundManager.PlayItemPickup();// звук взятия предмета
                Invoke(nameof(HideObject), 0.05f);

                CountItems.UpdateItemCountPanel(CountItems.itemCount - 1);

                itemCollected = true;
                flagInteract = false;
            }            
            else if (!gameInput.IsInteract())
                flagInteract = true;
        }
    }

    private void HideObject() => targetObject.SetActive(false);

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            flagInteract = false;
    }

    public bool ItemCollected() => itemCollected;
}
