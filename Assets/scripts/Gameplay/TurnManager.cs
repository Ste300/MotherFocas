
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour {
    public List<UnitController> teamA;
    public List<UnitController> teamB;
    int currentTeam = 0; // 0 = A, 1 = B
    int indexInTeam = 0;

    public void StartMatch() {
        StartCoroutine(HandleTurns());
    }

    IEnumerator HandleTurns() {
        while(!IsMatchOver()) {
            var currentUnit = GetNextAliveUnit();
            if(currentUnit == null) yield break;

            // Habilitar input para apuntar
            yield return StartCoroutine(DoPlayerTurn(currentUnit));
            // Cambiar turno
            AdvanceTurn();
        }
        // Resultado de la partida
    }

    UnitController GetNextAliveUnit() {
        List<UnitController> team = (currentTeam==0)?teamA:teamB;
        for(int i=0;i<team.Count;i++) {
            int idx = (indexInTeam + i) % team.Count;
            if(team[idx].isAlive) {
                indexInTeam = (idx + 1) % team.Count;
                return team[idx];
            }
        }
        return null;
    }

    IEnumerator DoPlayerTurn(UnitController unit) {
        
        yield return null;
    }

    void AdvanceTurn() {
        currentTeam = 1 - currentTeam;
    }

    bool IsMatchOver() {
        bool teamAAlive = teamA.Exists(u=>u.isAlive);
        bool teamBAlive = teamB.Exists(u=>u.isAlive);
        return !(teamAAlive && teamBAlive);
    }
    
}
