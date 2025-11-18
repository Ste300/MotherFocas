using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using UnityEngine.Android; 

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs & Spawns")]
    public NetworkObject playerPrefab;
    public List<Transform> spawnPoints;

    [Header("Session")]
    public GameMode mode = GameMode.AutoHostOrClient;
    public string sessionName = "Focas";
    public int maxPlayers = 2;

    [Header("UI References")]
    [Tooltip("Drag your UIManager here so we can show the menu on disconnect")]
    public UIManager uiManager;

    NetworkRunner _runner;
    
    readonly Dictionary<PlayerRef, NetworkObject> _players = new();
    NetworkTurnManager _turnMgr;
    [System.Serializable] public class IntEvent : UnityEngine.Events.UnityEvent<int> {}
    public IntEvent onPlayerCountChanged;

    [Header("Input (Legacy)")]
    public TeamManager teamManager;
    public LayerMask unitMask;
    public LayerMask groundMask;
    public float maxDragDistance = 6f;
    public Camera inputCamera;
    UnitController _hoverUnit; 
    public UnityEngine.Events.UnityEvent onMatchReady;

    // Ensures a NetworkRunner exists on Awake.
    void Awake()
    {
        GetOrCreateRunner();
    }

    // Checks if a NetworkRunner exists. If not, creates and configures a new one.
    private void GetOrCreateRunner()
    {
        if (_runner != null)
            return;

        _runner = GetComponent<NetworkRunner>();
        
        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();

        Debug.Log("NetworkRunner component ensured/created.");

        var sceneMgr = _runner.GetComponent<INetworkSceneManager>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>();
        _runner.ProvideInput = true;
        
        _runner.AddCallbacks(this);
    }

    // Asynchronously starts the Fusion matchmaking process.
    public async System.Threading.Tasks.Task StartMatchmaking()
    {
        GetOrCreateRunner(); 

        var args = new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = _runner.GetComponent<INetworkSceneManager>(),
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        };
        await _runner.StartGame(args);
        _turnMgr = FindObjectOfType<NetworkTurnManager>();
    }

    // UI button callback to start matchmaking.
    public void StartMatchmakingUI()
    {
        Debug.Log("[Spawner] StartMatchmaking pressed");
        GetOrCreateRunner(); 
        _ = StartMatchmaking();
        if (_runner != null && _runner.IsRunning) return;
    }

    // Called by Fusion when a new player joins the session.
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Spawner] PlayerJoined {player}");
        if (playerPrefab == null) return;
        if (_players.Count >= maxPlayers) return;

        int slot = _players.Count;
        var pos = GetSpawn(slot);
        var obj = runner.Spawn(playerPrefab, pos, Quaternion.identity, player);
        _players[player] = obj;
        onPlayerCountChanged?.Invoke(_players.Count);

        if (_players.Count == maxPlayers)
            onMatchReady?.Invoke();

        var info = obj.GetComponent<PlayerNetworkInfo>();
        if (info && obj.HasStateAuthority)
            info.Slot = slot;

        if (runner.IsServer && _players.Count == maxPlayers)
        {
            if (_turnMgr == null) _turnMgr = FindObjectOfType<NetworkTurnManager>();
            _turnMgr?.StartMatchRandom();
        }
    }

    // Called by Fusion when a player leaves the session.
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Spawner] PlayerLeft {player}");
        if (_players.TryGetValue(player, out var obj))
        {
            runner.Despawn(obj);
            _players.Remove(player);
        }
    }

    // Gets the appropriate spawn point for a player slot.
    Vector3 GetSpawn(int slot)
    {
        if (spawnPoints != null && spawnPoints.Count > slot && spawnPoints[slot])
            return spawnPoints[slot].position;
        return slot == 0 ? new Vector3(-3, 0, 0) : new Vector3(3, 0, 0);
    }

    // Called by Fusion when the runner shuts down. Cleans up and returns to the main menu.
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    { 
        Debug.Log($"[Spawner] Shutdown: {shutdownReason}");

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        if (uiManager != null)
        {
            uiManager.ShowMainMenu();
        }
        else
        {
            Debug.LogError("[BasicSpawner] UIManager not found! Cannot return to menu.");
        }

        _players.Clear();
        onPlayerCountChanged?.Invoke(0);

        if (_runner != null)
        {
            Debug.Log("Cleaning callbacks and destroying the spent NetworkRunner...");
            _runner.RemoveCallbacks(this);
            Destroy(_runner);
            _runner = null;
        }
    }

    // --- Other Callbacks (Empty) ---
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) {Debug.Log("[Spawner] DisconnectedFromServer"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log($"[Spawner] ConnectFailed: {reason} @ {remoteAddress}");}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef playerRef) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef playerRef) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> dictionary){ }
}