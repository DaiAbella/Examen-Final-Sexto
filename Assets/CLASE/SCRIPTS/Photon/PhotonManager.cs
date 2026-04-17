using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{

    public static PhotonManager _PhotonManager;

    [SerializeField] private NetworkRunner runner;
    [SerializeField] private NetworkPrefabRef prefab;
    [SerializeField] private NetworkSceneManagerDefault sceneManager;
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private UnityEvent onPlayerJoined;
    [SerializeField] private GameObject menuCanvas;

    public List<SessionInfo> availableSessions = new List<SessionInfo>();

    public event Action onSessionListUpdated;
    public static PhotonManager photonManager;
    public bool isHost;

    private void Awake()
    {
        if (_PhotonManager == null)
        {
            _PhotonManager = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        runner.AddCallbacks(this);
    }

    public async void JoinLobby()
    {
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            JoinLobby();
        }
    }

    private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    #region Métodos de Photon

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        Vector2 moveInput = InputManager.Instance.GetMoveInput();
        data.move = moveInput;

        data.look = InputManager.Instance.GetMouseDelta();
        data.isRunning = InputManager.Instance.WasRunInputPressed();

        if (Camera.main != null)
            data.yRotation = Camera.main.transform.eulerAngles.y;

        data.shoot = InputManager.Instance.ShootInputPressed();

        if (data.shoot && Camera.main != null)
            data.fireDirection = Camera.main.transform.forward;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            int randomSpawn = UnityEngine.Random.Range(0, spawnPoint.Length);

            NetworkObject networkPlayer = runner.Spawn(
     prefab,
     spawnPoint[randomSpawn].position,
     spawnPoint[randomSpawn].rotation,
     player,
     (runner, obj) =>
     {
         var pn = obj.GetComponent<PlayerNetwork>();
         if (pn != null)
         {
             pn.WeaponColor = PlayfabManager.SavedWeaponColor;
             Debug.Log("[PhotonManager] WeaponColor asignado al jugador: " + PlayfabManager.SavedWeaponColor);
         }
         else
         {
             Debug.LogError("[PhotonManager] PlayerNetwork NO encontrado en el prefab del jugador.");
         }
     }
 );


            players.Add(player, networkPlayer);

            if (GameManager.Instance != null)
                GameManager.Instance.CheckPlayersAndStart(runner);
        }

        if (onPlayerJoined != null)
            onPlayerJoined.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (players.TryGetValue(player, out NetworkObject networkPlayer))
        {
            runner.Despawn(networkPlayer);
            players.Remove(player);
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        availableSessions = sessionList;
        Debug.Log("Sesion nueva");
        onSessionListUpdated.Invoke();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    #endregion

    private async void StartGame(GameMode mode)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = scene,
            SceneManager = sceneManager,
            IsVisible = true,
        });
    }

    public async void JoinSession(string sessionName)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = sceneManager,
            IsVisible = true,
        });

        // 🔥 APAGAR MENÚ AL ENTRAR COMO CLIENTE
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }


    public void StartGameAsHost()
    {
        StartGame(GameMode.Host);

        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }


    public void StartGameAsClient()
    {
        StartGame(GameMode.Client);
    }

    private string RandomSessionName(int sessionNameLenght)
    {
        string caracteres = "abecdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        string sessionName = "";

        for (int i = 0; i < sessionNameLenght; i++)
        {
            char randomChar = caracteres[UnityEngine.Random.Range(0, caracteres.Length)];
            sessionName += randomChar;
        }

        return sessionName;
    }

    // 🔥🔥🔥 MÉTODO NUEVO PARA CREAR LOBBY PERSONALIZADO 🔥🔥🔥
    // 🔥 MÉTODO NUEVO PARA CREAR LOBBY PERSONALIZADO
    public async void CreateCustomLobby(string sessionName, int maxPlayers)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);

        await runner.StartGame(new StartGameArgs()

        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            Scene = scene,
            SceneManager = sceneManager,
            IsVisible = true,
        });

        if (menuCanvas != null)
            menuCanvas.SetActive(false);

        Debug.Log("Lobby creado: " + sessionName);
    }

}
