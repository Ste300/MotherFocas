using UnityEngine;
using Fusion;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject))]
public class NetworkTurnManager : NetworkBehaviour
{
    [Networked] public int ActiveSlot { get; private set; }
    [Networked] public TickTimer TurnTimer { get; private set; }
    [Networked] public NetworkBool GameStarted { get; private set; }
    [Networked] private int LastChangeTick { get; set; }
    [SerializeField] float turnSeconds = 10f;

    [Header("Game State")]
    [Networked] public NetworkBool IsGameFinished { get; private set; }
    [Networked] public int WinnerSlot { get; private set; } // -1 = Draw, 0 = Player 1, 1 = Player 2

    private TeamManager _teamManager;

    // Called by Fusion when this object spawns. Initializes game state.
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            _teamManager = FindObjectOfType<TeamManager>();
            if (_teamManager == null)
                Debug.LogError("[NetworkTurnManager] Host could not find TeamManager!");
        }
        
        WinnerSlot = -1;
        IsGameFinished = false;
    }

    // (HOST ONLY) Starts the match, sets a random starting player, and resets state.
    public void StartMatchRandom()
    {
        if (!Object.HasStateAuthority) return;

        ActiveSlot = Random.Range(0, 2);
        var secs = Mathf.Max(0.5f, turnSeconds);
        TurnTimer = TickTimer.CreateFromSeconds(Runner, secs);
        GameStarted = true;
        LastChangeTick = Runner.Tick;

        IsGameFinished = false;
        WinnerSlot = -1;
    }

    // (HOST ONLY) Immediately advances the turn to the other player.
    public void ForceNextTurn()
    {
        if (!Object.HasStateAuthority) return;
        if (Runner.Tick == LastChangeTick) return; 

        ActiveSlot = 1 - ActiveSlot; 
        var secs = Mathf.Max(0.5f, turnSeconds);
        TurnTimer = TickTimer.CreateFromSeconds(Runner, secs);
        LastChangeTick = Runner.Tick;
    }

    // (HOST ONLY) Forces the turn to a specific slot (0 or 1).
    public void SetTurnSlot(int slot)
    {
        if (!Object.HasStateAuthority) return;
        if (Runner.Tick == LastChangeTick) return; 

        ActiveSlot = Mathf.Clamp(slot, 0, 1);
        var secs = Mathf.Max(0.5f, turnSeconds);
        TurnTimer = TickTimer.CreateFromSeconds(Runner, secs);
        LastChangeTick = Runner.Tick;
        
        Debug.Log($"[NetworkTurnManager] Turn forced to Slot {ActiveSlot}");
    }

    // Checks if the given slot matches the active turn slot.
    public bool IsMyTurn(int mySlot) => mySlot == ActiveSlot;

    // Gets the remaining time on the turn timer.
    public float GetTimeLeftSeconds()
    {
        if (TurnTimer.IsRunning) return (float)TurnTimer.RemainingTime(Runner);
        return 0f;
    }

    // Main game loop tick. Checks for game end and expired timers.
    public override void FixedUpdateNetwork()
    {
        if (IsGameFinished) return;

        if (!Object.HasStateAuthority) return;

        CheckForGameEnd();

        if (!TurnTimer.IsRunning) return;

        if (TurnTimer.Expired(Runner))
            ForceNextTurn();
    }
    
    // (HOST ONLY) Checks TeamManager lists to see if a winner has been decided.
    private void CheckForGameEnd()
    {
        if (!Object.HasStateAuthority) return;
        if (_teamManager == null) return;

        int aliveTeam1 = CountAlive(_teamManager.team1);
        int aliveTeam2 = CountAlive(_teamManager.team2);

        if (aliveTeam1 > 0 && aliveTeam2 > 0)
        {
            return;
        }
        
        if (aliveTeam1 <= 0 && aliveTeam2 > 0)
        {
            IsGameFinished = true;
            WinnerSlot = 1; 
        }
        else if (aliveTeam2 <= 0 && aliveTeam1 > 0)
        {
            IsGameFinished = true;
            WinnerSlot = 0;
        }
        else if (aliveTeam1 <= 0 && aliveTeam2 <= 0)
        {
            IsGameFinished = true;
            WinnerSlot = -1;
        }
    }

    // Helper function to count living units in a list.
    int CountAlive(List<UnitController> list)
    {
        if (list == null) return 0;
        int c = 0;
        foreach (var u in list)
        {
            if (u != null && u.isAlive && u.gameObject.activeInHierarchy) 
            {
                c++;
            }
        }
        return c;
    }
}