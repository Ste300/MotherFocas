using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager_Polling : MonoBehaviour
{
    [Header("Team 1")]
    [Tooltip("Units for Team 1, in the same order as the icons")]
    public List<UnitController> team1Units = new List<UnitController>();
    [Tooltip("UI Icons (Images) for Team 1")]
    public List<Image> team1Icons = new List<Image>();

    [Header("Team 2")]
    public List<UnitController> team2Units = new List<UnitController>();
    public List<Image> team2Icons = new List<Image>();

    [Header("Options")]
    public float pollInterval = 0.5f;
    public bool useGreyOutInsteadOfHide = false;

    // Starts the polling coroutine when enabled.
    void OnEnable()
    {
        StartCoroutine(PollCoroutine());
    }

    // Stops the coroutine when disabled.
    void OnDisable()
    {
        StopAllCoroutines();
    }

    // Periodically checks the health of units and updates UI icons.
    IEnumerator PollCoroutine()
    {
        UpdateIcons(team1Units, team1Icons);
        UpdateIcons(team2Units, team2Icons);

        while (true)
        {
            yield return new WaitForSeconds(pollInterval);
            UpdateIcons(team1Units, team1Icons);
            UpdateIcons(team2Units, team2Icons);
        }
    }

    // Updates a list of icons based on the health of a list of units.
    void UpdateIcons(List<UnitController> units, List<Image> icons)
    {
        if (icons == null || icons.Count == 0) return;

        for (int i = 0; i < icons.Count; i++)
        {
            var img = icons[i];
            if (img == null) continue;

            bool visible = false;
            if (units != null && i < units.Count && units[i] != null)
            {
                visible = units[i].isAlive;
            }

            if (!useGreyOutInsteadOfHide)
            {
                img.gameObject.SetActive(visible);
            }
            else
            {
                img.gameObject.SetActive(true);
                Color c = img.color;
                c.a = visible ? 1f : 0.35f;
                img.color = c;
            }
        }
    }
}