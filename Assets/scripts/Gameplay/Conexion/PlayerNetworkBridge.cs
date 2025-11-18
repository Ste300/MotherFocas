using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkBridge : NetworkBehaviour
{
    private TeamManager _teamManager;
    private PlayerNetworkInfo _localInfo;
    private NetworkTurnManager _networkTurnManager;

    // Finds the TeamManager on Awake.
    void Awake()
    {
        _teamManager = FindObjectOfType<TeamManager>();
    }

    // Finds other managers and local info on Start.
    void Start()
    {
        if (_teamManager == null)
            _teamManager = FindObjectOfType<TeamManager>();
        
        _networkTurnManager = FindObjectOfType<NetworkTurnManager>();
        _localInfo = GetComponent<PlayerNetworkInfo>();
    }

    // Helper to safely get the TeamManager reference.
    private TeamManager TeamManagerOrLookup()
    {
        if (_teamManager == null) _teamManager = FindObjectOfType<TeamManager>();
        return _teamManager;
    }

    // [RPC] Client requests the Host to launch a seal.
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RpcLaunchSeal(NetworkObject sealObj, Vector3 dir, float power)
    {
        if (sealObj == null) return;
        var unit = sealObj.GetComponent<UnitController>();
        var tm = TeamManagerOrLookup();
        
        if (_networkTurnManager == null)
            _networkTurnManager = FindObjectOfType<NetworkTurnManager>();
        
        if (unit == null || tm == null || _networkTurnManager == null) 
        {
             Debug.LogError("[Host] Missing references (Unit, TM or NetTurnMgr) in RpcLaunchSeal");
             return;
        }

        if (_localInfo == null)
        {
            Debug.LogError($"[Host] PlayerNetworkBridge (from player {Object.InputAuthority}) has no _localInfo.");
            return;
        }

        int senderSlot = _localInfo.Slot;

        // --- HOST VALIDATION ---
        if (!tm.IsSealOwnedBySlot(unit, senderSlot))
        {
            Debug.LogWarning($"[Host] RPC REJECTED: Player (Slot {senderSlot}) tried to move a unit they don't own.");
            return;
        }

        if (_networkTurnManager.ActiveSlot != senderSlot)
        {
            Debug.LogWarning($"[Host] RPC REJECTED: Player (Slot {senderSlot}) tried to play out of turn.");
            return;
        }

        if (!unit.IsAvailable || unit.IsLaunched)
        {
            Debug.LogWarning($"[Host] RPC REJECTED: Player (Slot {senderSlot}) tried to move an unavailable unit.");
            return;
        }

        // --- RPC ACCEPTED ---
        unit.Launch(dir.normalized, Mathf.Clamp01(power));
        unit.MarkUsed();
        tm.OnSealLaunched(unit); 
    }

    // [RPC] Client requests the Host to set a seal's rotation.
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RpcSetSealYaw(NetworkObject sealObj, float yaw)
    {
        if (sealObj == null) return;
        var unit = sealObj.GetComponent<UnitController>();
        if (unit == null) return;

        unit.SetTargetYaw(yaw);
    }
}