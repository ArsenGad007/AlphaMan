using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private ItemTrigger itemTrigger;
    [SerializeField] private GameWin gameWin;
    [SerializeField] private string playerTag = "Player";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            if (itemTrigger.ItemCollected())
                gameWin.GameWinPanel();
    }
}
