using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private NetworkPrefabRef prefab;
    [SerializeField] NetworkSceneManagerDefault sceneManager;
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] UnityEvent onPlayerJoined;

    Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();


    #region Metodos de Photon



    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Movimiento: si no hay input, usar Vector2.zero
        Vector2 moveInput = InputManager.Instance.GetMoveInput();
        data.move = moveInput == null ? Vector2.zero : moveInput;

        // Rotación con el mouse
        data.look = InputManager.Instance.GetMouseDelta();

        // Estado de correr
        data.isRunning = InputManager.Instance.WasRunInputPressed();

        // Rotación en Y de la cámara principal
        if (Camera.main != null)
            data.yRotation = Camera.main.transform.eulerAngles.y;

        // Disparo
        data.shoot = InputManager.Instance.ShootInputPressed();

        // Enviar el input al runner
        input.Set(data);
    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            int randomSpawn = UnityEngine.Random.Range(0, spawnPoint.Length);
            NetworkObject networkPlayer = runner.Spawn(prefab, spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation, player);
            players.Add(player, networkPlayer);
        }

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

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
    #endregion

    private async void StartGame(GameMode mode)
    {
        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        var Scene = SceneRef.FromIndex(0);

        var SceneInfo = new NetworkSceneInfo();

        if (Scene.IsValid)
        {
            SceneInfo.AddSceneRef(Scene, LoadSceneMode.Additive);
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = Scene,
            CustomLobbyName = "Official EA Europe",
            SceneManager = sceneManager

        });
    }


    public void StartGameAsHost()
    {
        StartGame(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        StartGame(GameMode.Client);
    }
}
