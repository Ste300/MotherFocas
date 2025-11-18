using UnityEngine;

public static class CombatSystem
{
    // Resolves damage when two units collide. Must be called on Host.
    public static void ResolveCollision(UnitController attacker, UnitController defender)
    {
        if (attacker == null || defender == null) return;

        int dmg = 1;
        if (attacker.data != null)
            dmg = Mathf.Max(1, Mathf.RoundToInt(attacker.data.attackPower));

        var defenderCombat = defender.GetComponent<NetworkSealCombat>();
        if (defenderCombat != null)
        {
            defenderCombat.ApplyDamageTo(dmg);
        }
    }
}