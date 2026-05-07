using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private ItemTrigger[] requiredItems; // массив предметов, которые нужно собрать
    [SerializeField] private GameWin gameWin;
    [SerializeField] private string playerTag = "Player";

    private static GameObject spriteObj;

    private void Awake()
    {
        if (transform.childCount > 0)
            spriteObj = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        if (spriteObj != null)
            spriteObj.SetActive(false);

        CountItems.UpdateItemCountPanel(requiredItems.Length);     
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && AllItemsCollected())
            gameWin.GameWinPanel();
    }

    private bool AllItemsCollected()
    {
        foreach (ItemTrigger item in requiredItems)
            if (item != null && !item.ItemCollected())          
                return false;

        return true;
    }

    /// <summary>
    /// ѕоказывает область куда зайти дл€ победы
    /// </summary>
    public static void ShowAriaWin()
    {
        spriteObj.SetActive(true);
    }
}
