using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class TeamManager : MonoBehaviour
{
    [Header("Teams")]
    public List<UnitController> team1;
    public List<UnitController> team2;

    private NetworkTurnManager _networkTurnManager;

    [Header("State")]
    private bool waitingForSealToStop = false;
    private int currentRound = 0;
    public int CurrentRound => currentRound;

    private Dictionary<UnitController, Vector3> _initialPositions = new Dictionary<UnitController, Vector3>();
    private Dictionary<UnitController, Quaternion> _initialRotations = new Dictionary<UnitController, Quaternion>();

    // Finds the NetworkTurnManager and prepares the first round.
    void Start()
    {
        _networkTurnManager = FindObjectOfType<NetworkTurnManager>();
        if (_networkTurnManager == null)
            Debug.LogError("TeamManager could not find NetworkTurnManager!");

        RecordInitialUnitStates();
        PrepareNewRound();
    }

    // Stores the starting positions of all units.
    private void RecordInitialUnitStates()
    {
        _initialPositions.Clear();
        _initialRotations.Clear();

        List<UnitController> all = new List<UnitController>();
        if (team1 != null) all.AddRange(team1);
        if (team2 != null) all.AddRange(team2);

        foreach (var u in all)
        {
            if (u == null) continue;
            _initialPositions[u] = u.transform.position;
            _initialRotations[u] = u.transform.rotation;
        }
    }
    
    // Resets all units to their initial state (used by Host).
    public void ResetToInitialState()
    {
        List<UnitController> all = new List<UnitController>();
        if (team1 != null) all.AddRange(team1);
        if (team2 != null) all.AddRange(team2);

        foreach (var u in all)
        {
            if (u == null) continue;
            if (!u.gameObject.activeInHierarchy)
                u.gameObject.SetActive(true);

            if (u.data != null) u.currentHP = u.data.maxHP;
            else u.currentHP = Mathf.Max(1, u.currentHP);

            u.hasBeenUsed = false;

            if (_initialPositions.ContainsKey(u))
                u.transform.position = _initialPositions[u];
            if (_initialRotations.ContainsKey(u))
                u.transform.rotation = _initialRotations[u];

            Rigidbody rb = u.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            try { u.ResetLaunchState(); } catch { }
            try { u.ResetForNewRound(); } catch { }
        }
        
        RecordInitialUnitStates();
    }
    
    // Checks if the Host is ready to accept a new launch RPC.
    public bool CanCurrentTeamPlay()
    {
        return !waitingForSealToStop;
    }

    // Checks if the seal belongs to the *network-active* team.
    public bool IsSealFromCurrentTeam(UnitController seal)
    {
        if (seal == null || _networkTurnManager == null) return false;

        int activeSlot = _networkTurnManager.ActiveSlot; 

        if (activeSlot == 0)
            return team1 != null && team1.Contains(seal);
        else 
            return team2 != null && team2.Contains(seal);
    }

    // Logs the selected seal.
    public void SelectSeal(UnitController seal)
    {
        Debug.Log($"Selected {seal?.name}");
    }

    // Called by the Host when a seal is launched.
    public void OnSealLaunched(UnitController seal)
    {
        StartCoroutine(WaitForSealToStopCoroutine(seal));
    }

    // (HOST ONLY) Waits for a launched seal to stop, then decides to end round or pass turn.
    private IEnumerator WaitForSealToStopCoroutine(UnitController seal)
    {
        waitingForSealToStop = true;

        while (seal != null && seal.isAlive && seal.IsLaunched)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        if (_networkTurnManager != null)
        {
            if (AllSealsUsed())
            {
                Debug.Log("[TeamManager] All seals used! Preparing new round.");
                PrepareNewRound();
                _networkTurnManager.SetTurnSlot(0);
            }
            else
            {
                _networkTurnManager.ForceNextTurn();
            }
        }
        
        waitingForSealToStop = false;
    }

    // Checks if all units on both teams have been used.
    private bool AllSealsUsed()
    {
        if (team1 != null)
        {
            foreach (var s in team1)
                if (s != null && s.IsAvailable) return false;
        }
        if (team2 != null)
        {
            foreach (var s in team2)
                if (s != null && s.IsAvailable) return false;
        }
        return true;
    }

    // Resets all units' 'hasBeenUsed' flags for a new round.
    public void PrepareNewRound()
    {
        currentRound++;
        Debug.Log($"Starting Round #{currentRound}");

        if (team1 != null)
        {
            foreach (var s in team1) if (s != null) s.ResetForNewRound();
        }
        if (team2 != null)
        {
            foreach (var s in team2) if (s != null) s.ResetForNewRound();
        }

        waitingForSealToStop = false;
    }
   
    // Returns the team ID (1 or 2) of a given unit.
    public int GetTeamOfUnit(UnitController u)
    {
        if (u == null) return 0;
        if (team1 != null && team1.Contains(u)) return 1;
        if (team2 != null && team2.Contains(u)) return 2;
        return 0;
    }

    // Checks if a unit belongs to a specific player slot.
    public bool IsSealOwnedBySlot(UnitController seal, int slot)
    {
        if (seal == null) return false;
        
        if (slot == 0)
        {
            return team1 != null && team1.Contains(seal);
        }
        else if (slot == 1)
        {
            return team2 != null && team2.Contains(seal);
        }
        return false;
    }
}