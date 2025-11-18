using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(UnitController))]
public class NetworkSealCombat : NetworkBehaviour
{
    [Networked] public int NetHP { get; set; }

    private int _lastSeenHP = int.MinValue;
    UnitController _unit;

    // Caches the local UnitController.
    void Awake()
    {
        _unit = GetComponent<UnitController>();
    }

    // (HOST) Ensures NetHP is initialized and synchronized with the local unit's HP.
    public override void FixedUpdateNetwork()
    {
        if (Object == null) return;

        if (Object.HasStateAuthority)
        {
            if (NetHP == 0 && _unit != null)
            {
                NetHP = Mathf.Max(1, _unit.currentHP);
            }

            if (_unit != null && NetHP != _unit.currentHP)
                NetHP = _unit.currentHP;
        }
    }

    // (CLIENT) Watches for changes in NetHP and applies them to the local unit.
    void Update()
    {
        if (!Object.HasStateAuthority)
        {
            if (_lastSeenHP != NetHP)
            {
                _lastSeenHP = NetHP;
                if (_unit != null)
                    _unit.SetHPLocal(_lastSeenHP);
            }
        }
    }

    // (HOST ONLY) Applies damage to this unit and updates NetHP.
    public void ApplyDamageTo(int dmg)
    {
        if (!Object.HasStateAuthority) return;
        if (_unit == null) return;

        int newHp = Mathf.Max(0, _unit.currentHP - dmg);
        _unit.currentHP = newHp;

        if (newHp <= 0)
        {
            _unit.DieLocal();
        }
        
        NetHP = _unit.currentHP;
    }
}