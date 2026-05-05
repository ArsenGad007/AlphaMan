using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUpgrades : MonoBehaviour
{
    [SerializeField] private GameObject upgradesPanel;
    
    [SerializeField] private Button enterUpgradesPanel;
    [SerializeField] private Button exitUpgradesPanel;
    [SerializeField] private Button speedUpgrade;

    [SerializeField] private TextMeshProUGUI coinsTotal;

    [SerializeField] private Image[] progressBar;

    private TextMeshProUGUI speedUpgradeText;
    private byte progressBarStep = 0;

    private void Awake()
    {
        speedUpgradeText = speedUpgrade.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        SavesLogic.Set("progress_bar_size", progressBar.Length);

        upgradesPanel.SetActive(false);

        enterUpgradesPanel.onClick.RemoveListener(EnterUpgrades);
        enterUpgradesPanel.onClick.AddListener(EnterUpgrades);
    }

    private void UpdateProgressBar()
    {
        for (int i = 0; i < progressBar.Length; i++)
        {
            if ((SavesLogic.Get("upgrade_speed_price", 10) - 10) / 10 > i)
                progressBar[i].color = new Color32(255, 200, 0, 255);   // Желтый
            else
                progressBar[i].color = new Color32(135, 135, 135, 255); // Серый
        }
    }

    private void EnterUpgrades()
    {
        upgradesPanel.SetActive(true);

        coinsTotal.text = SavesLogic.Get("coins_total", 0).ToString();
        speedUpgradeText.text = SavesLogic.Get("upgrade_speed_price", 10).ToString() + " $";

        UpdateProgressBar();

        exitUpgradesPanel.onClick.RemoveListener(ExitUpgrades);
        exitUpgradesPanel.onClick.AddListener(ExitUpgrades);

        speedUpgrade.onClick.RemoveListener(SpeedUpgrade);
        speedUpgrade.onClick.AddListener(SpeedUpgrade);
    }

    private void ExitUpgrades()
    {
        exitUpgradesPanel.onClick.RemoveListener(ExitUpgrades);
        speedUpgrade.onClick.RemoveListener(SpeedUpgrade);

        upgradesPanel.SetActive(false);
    }

    private void SpeedUpgrade()
    {
        int coins_total = SavesLogic.Get("coins_total", 0);
        int upgrade_speed_price = SavesLogic.Get("upgrade_speed_price", 10);

        if (progressBar.Length > progressBarStep && coins_total >= upgrade_speed_price)
        {
            SavesLogic.Set("coins_total", coins_total - upgrade_speed_price);
            SavesLogic.Set("upgrade_speed_price", upgrade_speed_price + 10);

            coinsTotal.text = SavesLogic.Get("coins_total", 0).ToString();
            speedUpgradeText.text = SavesLogic.Get("upgrade_speed_price", 10).ToString() + " $";

            List<ISpeedUpgradable> obj = new() { PlayerAnimator.Instance, PlayerController.Instance, SoundManager.Instance };
            foreach (ISpeedUpgradable _obj in obj)
                _obj.SpeedProgressUpdate();

            UpdateProgressBar();

            progressBarStep++;
        }
    }
}
