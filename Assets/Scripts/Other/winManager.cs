using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class winManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text endGameText, countDownText;
    [SerializeField] private GameObject endGameMenu;
    [SerializeField] private GameObject OtherUI1, OtherUI2, OtherUI3;

    [Header("Game Settings")]
    public NetworkVariable<int> playerKills = new NetworkVariable<int>(0);
    public NetworkVariable<bool> EndGameStarted = new NetworkVariable<bool>();
    public NetworkVariable<float> EndGameTimer = new NetworkVariable<float>(10f);
    private bool endGameTriggered = false;

    public override void OnNetworkSpawn()
    {
        // Initialize UI state
        countDownText.gameObject.SetActive(false);
        endGameMenu.SetActive(false);

        // Listen for network variable changes
        EndGameTimer.OnValueChanged += HandleTimerChanged;
        EndGameStarted.OnValueChanged += HandleEndGameStarted;
        playerKills.OnValueChanged += HandleKillsChanged;
    }

    private void HandleKillsChanged(int previous, int current)
    {
        // Update local kill count display
        if (IsOwner)
        {
            // Update your kill counter UI here if needed
        }
    }

    private void HandleEndGameStarted(bool previous, bool current)
    {
        if (current)
        {
            countDownText.gameObject.SetActive(true);
        }
    }

    private void HandleTimerChanged(float previous, float current)
    {
        if (EndGameStarted.Value)
        {
            countDownText.text = $"WARZONE ENDS IN: {(Mathf.Round((current*100)))/100}";
        }
    }

    private void Update()
    {
        if (!EndGameStarted.Value) return;

        // Server handles timer countdown
        if (IsServer && EndGameTimer.Value > 0)
        {
            EndGameTimer.Value -= Time.deltaTime;
        }

        if (EndGameTimer.Value <= 0 && !endGameTriggered)
        {
            endGameTriggered = true;
            TriggerEndGame();
        }

    }

    private void TriggerEndGame()
    {
        // Determine if this player is a winner
        bool isWinner = (playerKills.Value >= GameLoader.Instance.maxKills.Value);

        // Tell client to show end game
        ShowEndGame(isWinner);
    }


    private void ShowEndGame(bool isWinner)
    {
        Time.timeScale = 0;
        countDownText.gameObject.SetActive(false);

        OtherUI1.SetActive(false);
        OtherUI2.SetActive(false);
        OtherUI3.SetActive(false);

        if (IsOwner)
        {
            endGameMenu.SetActive(true);
            endGameText.text = isWinner
                ? "You are the ruler of the Warzone 😁😁😁"
                : "You lost ☹☹☹";
        }
    }

    // Call this when the player gets a kill
    public void AddKill()
    {
        if (!IsOwner) return;

        // Report kill to server
        AddKillServerRpc();
    }

    [ServerRpc]
    private void AddKillServerRpc()
    {
        playerKills.Value++;

        // Update global max kills if needed
        if (playerKills.Value > GameLoader.Instance.maxKills.Value)
        {
            GameLoader.Instance.maxKills.Value = playerKills.Value;
        }
    }

    public override void OnDestroy()
    {
        EndGameTimer.OnValueChanged -= HandleTimerChanged;
        EndGameStarted.OnValueChanged -= HandleEndGameStarted;
        playerKills.OnValueChanged -= HandleKillsChanged;
        base.OnDestroy();
    }
}