using System.Linq;
using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Spawns")]
    [SerializeField] private Transform[] spawnPoints;

    [Networked] public int Timer { get; set; }
    [Networked, Capacity(16)] private NetworkArray<int> scores => default;
    [Networked, Capacity(16)] private NetworkArray<bool> retryVotes => default;

    private bool matchRunning = false;
    private double lastTickTime;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[GameManager] Awake: Instance asignada.");
    }

    public override void Spawned()
    {
        Debug.Log($"[GameManager] Spawned. StateAuthority={Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            Timer = 60;
            lastTickTime = Runner.SimulationTime;
            Debug.Log("[GameManager] Timer inicializado en host.");
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[GameManager] No hay spawnPoints asignados.");
            return null;
        }

        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    public void CheckPlayersAndStart(NetworkRunner runner)
    {
        int count = runner.ActivePlayers.Count();
        Debug.Log($"[GameManager] CheckPlayersAndStart. Players={count}, StateAuthority={Object.HasStateAuthority}");

        if (count >= 2 && Object.HasStateAuthority)
        {
            RpcStartMatch();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartMatch()
    {
        matchRunning = true;

        if (Object.HasStateAuthority)
        {
            Timer = 60;
            lastTickTime = Runner.SimulationTime;
            Debug.Log("[GameManager] RpcStartMatch: Timer reiniciado en host.");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStartPanel(false);
            UIManager.Instance.ShowGameCanvas(true);
            UIManager.Instance.ShowEndPanel(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!matchRunning || !Object.HasStateAuthority || Timer <= 0) return;

        if (Runner.SimulationTime - lastTickTime >= 1.0)
        {
            Timer--;
            lastTickTime = Runner.SimulationTime;
            Debug.Log($"[GameManager] Timer actualizado: {Timer}");

            if (Timer <= 0)
            {
                RpcEndMatch();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcEndMatch()
    {
        matchRunning = false;
        Debug.Log("[GameManager] RpcEndMatch: mostrando ganador.");

        int maxScore = -1;
        int winnerIndex = -1;

        for (int i = 0; i < scores.Length; i++)
        {
            int s = scores.Get(i);
            if (s > maxScore)
            {
                maxScore = s;
                winnerIndex = i;
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameCanvas(false);
            UIManager.Instance.ShowEndMatch(winnerIndex, maxScore);
            UIManager.Instance.ShowEndPanel(true);
        }
    }

    public void AddKill(PlayerRef killer)
    {
        if (!Object.HasStateAuthority) return;
        int idx = killer.RawEncoded;
        scores.Set(idx, scores.Get(idx) + 1);
    }

    public int GetScore(PlayerRef player) => scores.Get(player.RawEncoded);

    public void RegisterRetryVote(PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;
        int idx = player.RawEncoded;
        retryVotes.Set(idx, true);
        CheckAllVotesAndRestart();
    }

    private void CheckAllVotesAndRestart()
    {
        foreach (var p in Runner.ActivePlayers)
        {
            if (!retryVotes.Get(p.RawEncoded))
                return;
        }

        for (int i = 0; i < retryVotes.Length; i++)
            retryVotes.Set(i, false);

        RpcStartMatch();
    }
}
