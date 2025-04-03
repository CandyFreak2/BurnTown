using Fusion;
using TMPro;
using UnityEngine;

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

        if (!wasSaved && GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.END && Runner.IsServer) {
            PlayerPrefs.SetInt("N_FinalScore", overallScore);
            wasSaved = true;    
        }
    }

    public void UpdateScoreUI(int score) {
        overallScore += score;
        scoreText.text = SCORE_HEADER + overallScore.ToString("C0");
    }

    private void OnEnable() {
        EventManager.onBurnableDestroyed += UpdateScoreUI;
    }

    private void OnDisable() {
        EventManager.onBurnableDestroyed -= UpdateScoreUI;
    }
}
