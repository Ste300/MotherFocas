using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkInfo : NetworkBehaviour
{
    [Networked] public int Slot { get; set; }
}