using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private ItemTrigger[] requiredItems; // массив предметов, которые нужно собрать
    [SerializeField] private GameWin gameWin;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && AllItemsCollected())
            gameWin.GameWinPanel();
    }

    private bool AllItemsCollected()
    {
        foreach (ItemTrigger item in requiredItems)
        {
            if (item != null && !item.ItemCollected())
                return false;
        }
        return true;
    }
}
