using UnityEngine;
using Fusion;
using System.Linq;

public class PlayerInputController : MonoBehaviour
{
    public TeamManager teamManager;
    public AimSystem3D aimSystem; 

    private Camera cam;
    private UnitController selectedSeal;
    private bool isAiming = false;

    private PlayerNetworkBridge _myPlayerBridge = null;
    private PlayerNetworkInfo _myPlayerInfo = null;
    private NetworkTurnManager _networkTurnManager;

    // Caches main references.
    void Start()
    {
        cam = Camera.main;
        teamManager = FindObjectOfType<TeamManager>();
        if (aimSystem == null)
            aimSystem = FindObjectOfType<AimSystem3D>();
        
        _networkTurnManager = FindObjectOfType<NetworkTurnManager>();

        if (aimSystem != null)
            aimSystem.Hide();
    }

    // Handles player input state machine (Select, Aim, Launch).
    void Update()
    {
        if (teamManager == null) return;
        
        // Find network components if we don't have them
        if (_networkTurnManager == null)
        {
            _networkTurnManager = FindObjectOfType<NetworkTurnManager>();
            if (_networkTurnManager == null) return;
        }

        if (_myPlayerBridge == null)
        {
            _myPlayerBridge = FindObjectsOfType<PlayerNetworkBridge>()
                .FirstOrDefault(p => p.HasInputAuthority);
            
            if (_myPlayerBridge != null)
            {
                _myPlayerInfo = _myPlayerBridge.GetComponent<PlayerNetworkInfo>();
            }
        }
        
        if (_myPlayerBridge == null || _myPlayerInfo == null)
        {
            return; 
        }

        if (_networkTurnManager.IsGameFinished) return;

        // --- Input Logic ---

        // 1. Select Unit
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                UnitController seal = hit.collider.GetComponentInParent<UnitController>();
                
                if (seal == null) return;

                // --- LOCAL VALIDATION ---
                bool isMine = teamManager.IsSealOwnedBySlot(seal, _myPlayerInfo.Slot);
                bool isMyTurn = _networkTurnManager.ActiveSlot == _myPlayerInfo.Slot;
                bool isAvailable = seal.IsAvailable;
                
                if (isMine && isMyTurn && isAvailable)
                {
                    selectedSeal = seal;
                    teamManager.SelectSeal(seal);
                    isAiming = true;

                    if (aimSystem != null)
                    {
                        aimSystem.originTransform = seal.transform;
                        aimSystem.DrawAim(seal.transform.position);
                    }
                }
            }
        }

        // 2. Aiming (drag)
        if (isAiming && selectedSeal != null && Input.GetMouseButton(0))
        {
            if (aimSystem != null && aimSystem.ScreenPointToGroundPoint(Input.mousePosition, cam, out Vector3 worldPoint))
            {
                aimSystem.DrawAim(worldPoint, pullToLaunch: true);

                Vector3 dirVisual = aimSystem.GetDirection(worldPoint).normalized;
                Vector3 dirLaunch = -dirVisual;
                float yaw = Mathf.Atan2(dirLaunch.x, dirLaunch.z) * Mathf.Rad2Deg;

                var sealNetObj = selectedSeal.GetComponent<NetworkObject>();
                if (sealNetObj != null)
                    _myPlayerBridge.RpcSetSealYaw(sealNetObj, yaw);
            }
        }

        // 3. Launch (release)
        if (isAiming && selectedSeal != null && Input.GetMouseButtonUp(0))
        {
            if (aimSystem != null && aimSystem.ScreenPointToGroundPoint(Input.mousePosition, cam, out Vector3 worldPoint))
            {
                Vector3 dir = -aimSystem.GetDirection(worldPoint).normalized;
                float power = aimSystem.CalculateStrength(worldPoint);

                var sealNetObj = selectedSeal.GetComponent<NetworkObject>();
                if (sealNetObj != null)
                    _myPlayerBridge.RpcLaunchSeal(sealNetObj, dir, power);
            }

            if (aimSystem != null)
                aimSystem.Hide();

            isAiming = false;
            selectedSeal = null;
        }
    }
}