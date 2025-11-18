using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTeamData", menuName = "Game/TeamData", order = 10)]
public class TeamData : ScriptableObject
{
    [Tooltip("Numeric ID (1, 2,...)")]
    public int teamId = 0;

    [Tooltip("Display name for the team")]
    public string teamName = "Team";

    public List<UnitController> members = new List<UnitController>();
}