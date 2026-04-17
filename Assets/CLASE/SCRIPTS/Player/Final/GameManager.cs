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
            // ⏱ Tiempo inicial de la partida: 2 minutos (120s)
            Timer = 120;
            lastTickTime = Runner.SimulationTime;
            Debug.Log("[GameManager] Timer inicializado en host (120s).");
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
        if (Object == null || !Object.IsValid) return;

        int count = runner.ActivePlayers.Count();
        Debug.Log($"[GameManager] CheckPlayersAndStart. Players={count}, StateAuthority={Object.HasStateAuthority}");

        // 🔥 La partida solo empieza cuando hay 2 o más jugadores
        if (count >= 2 && Object.HasStateAuthority)
        {
            RpcStartMatch();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartMatch()
    {
        if (Object == null || !Object.IsValid) return;

        matchRunning = true;

        if (Object.HasStateAuthority)
        {
            // Reiniciamos el timer a 2 minutos al empezar la partida
            Timer = 120;
            lastTickTime = Runner.SimulationTime;
            Debug.Log("[GameManager] RpcStartMatch: Timer reiniciado en host (120s).");
        }

        // 👇 Ya no tocamos paneles aquí.
        // El UIManager se encarga de mostrar el GameUI siempre,
        // y solo mostramos el panel final al terminar.
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid) return;

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
        if (Object == null || !Object.IsValid) return;

        matchRunning = false;
        Debug.Log("[GameManager] RpcEndMatch: calculando ganador y mostrando resultado.");

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

        // 👇 Solo notificamos al UI del resultado.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEndMatch(winnerIndex, maxScore);
        }
    }

    public void AddKill(PlayerRef killer)
    {
        if (Object == null || !Object.IsValid) return;
        if (!Object.HasStateAuthority) return;

        int idx = killer.RawEncoded;
        scores.Set(idx, scores.Get(idx) + 1);
        Debug.Log($"[GameManager] AddKill → Player {idx} ahora tiene {scores.Get(idx)} puntos.");
    }

    public int GetScore(PlayerRef player)
    {
        if (Object == null || !Object.IsValid) return 0;
        return scores.Get(player.RawEncoded);
    }

    public void RegisterRetryVote(PlayerRef player)
    {
        if (Object == null || !Object.IsValid) return;
        if (!Object.HasStateAuthority) return;

        int idx = player.RawEncoded;
        retryVotes.Set(idx, true);
        CheckAllVotesAndRestart();
    }

    private void CheckAllVotesAndRestart()
    {
        if (Object == null || !Object.IsValid) return;

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
