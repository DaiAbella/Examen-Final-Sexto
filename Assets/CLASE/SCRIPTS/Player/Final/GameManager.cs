using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform[] spawnPoints;

    [Networked] public int Timer { get; set; }
    [Networked, Capacity(10)] private NetworkArray<int> scores => default;
    [Networked, Capacity(10)] private NetworkArray<bool> retryVotes => default;

    private bool matchRunning = false;
    private float lastTickTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetSpawnPoints(Transform[] points)
    {
        spawnPoints = points;
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No hay spawnPoints asignados en GameManager.");
            return null;
        }

        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    public void AddKill(PlayerRef killer)
    {
        int index = killer.RawEncoded;
        scores.Set(index, scores.Get(index) + 1);

        Debug.Log("📊 Marcador actualizado:");
        for (int i = 0; i < scores.Length; i++)
        {
            int playerScore = scores.Get(i);
            if (playerScore > 0)
            {
                Debug.Log($"Jugador {i} → {playerScore} puntos");
            }
        }
    }

    public int GetScore(PlayerRef player)
    {
        return scores.Get(player.RawEncoded);
    }

    public void StartMatch()
    {
        if (matchRunning) return;

        matchRunning = true;

        if (Object.HasStateAuthority)
        {
            Timer = 60; // 1 minuto
            lastTickTime = Time.time;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!matchRunning || !Object.HasStateAuthority || Timer <= 0) return;

        // ✅ Decrementar cada segundo real usando Time.time
        if (Time.time - lastTickTime >= 1f)
        {
            Timer--;
            lastTickTime = Time.time;

            if (Timer <= 0)
            {
                EndMatch();
            }
        }
    }

    private void EndMatch()
    {
        matchRunning = false;

        int maxScore = -1;
        int winnerIndex = -1;

        for (int i = 0; i < scores.Length; i++)
        {
            if (scores.Get(i) > maxScore)
            {
                maxScore = scores.Get(i);
                winnerIndex = i;
            }
        }

        Debug.Log($"🏆 Partida terminada. Ganador: Jugador {winnerIndex} con {maxScore} puntos.");
        UIManager.Instance.ShowEndMatch(winnerIndex, maxScore);
    }

    public void RegisterRetryVote(PlayerRef player)
    {
        int index = player.RawEncoded;
        retryVotes.Set(index, true);

        Debug.Log($"Jugador {index} votó Retry");

        CheckAllVotes();
    }

    private void CheckAllVotes()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            if (!retryVotes.Get(player.RawEncoded))
                return;
        }

        ResetVotes();
        StartMatch();
    }

    private void ResetVotes()
    {
        for (int i = 0; i < retryVotes.Length; i++)
        {
            retryVotes.Set(i, false);
        }
    }
}
