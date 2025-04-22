using Fusion;
using TMPro;
using UnityEngine;
using static GameStateHandler;

public class ScoreSystem : NetworkBehaviour {
    private const string SCORE_HEADER = "Financial Damage: ";

    [Networked, SerializeField] private int overallScore { get; set; }
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private bool isReset;

    private bool wasSaved = false;

    public override void Spawned() {
        scoreText.text = SCORE_HEADER + overallScore.ToString("C0");
    }

    public override void FixedUpdateNetwork() {
        if (scoreText != null && isReset) {
            isReset = false;
            overallScore = 0;
            scoreText.text = SCORE_HEADER + overallScore.ToString("C0");
        }

        if (!wasSaved && Instance.GetGameState == GameState.END && Runner.IsServer) {
            PlayerPrefs.SetInt("N_FinalScore", overallScore);
            wasSaved = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdateScore(int score) {
        overallScore += score;
        RPC_UpdateScoreUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateScoreUI() {
        scoreText.text = SCORE_HEADER + overallScore.ToString("C0");
    }

    private void OnEnable() {
        EventManager.onBurnableDestroyed += RPC_UpdateScore;
    }

    private void OnDisable() {
        EventManager.onBurnableDestroyed -= RPC_UpdateScore;
    }
}
