using Fusion;
using TMPro;
using UnityEngine;

public class ScoreSystem : NetworkBehaviour {
    private const string SCORE_HEADER = "Financial Damage: $";

    [Networked, SerializeField] private int overallScore { get; set; }
    private bool isSpawned = false;

    public TMP_Text scoreText;

    public override void Spawned() {
        isSpawned = true;
    }

    private void FixedUpdate() {
        if (scoreText == null && isSpawned) {
            scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
            scoreText.text = SCORE_HEADER + overallScore;
        }
    }

    public void UpdateScoreUI(int score) {
        overallScore += score;
        scoreText.text = SCORE_HEADER + overallScore;
    }

    private void OnEnable() {
        EventManager.OnBurnableDestroyed += UpdateScoreUI;
    }

    private void OnDisable() {
        EventManager.OnBurnableDestroyed -= UpdateScoreUI;
    }

}
