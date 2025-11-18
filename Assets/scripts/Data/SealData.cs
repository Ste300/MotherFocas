using UnityEngine;

[CreateAssetMenu(fileName = "NewSealData", menuName = "Game/Seal Data")]
public class SealData : ScriptableObject
{
    public string sealName;
    public int maxHP = 10;
    public float moveSpeed = 5f;
    public float attackPower = 2f;
}
