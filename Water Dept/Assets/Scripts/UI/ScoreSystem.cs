using Fusion;
using TMPro;
using UnityEngine;

public class ScoreSystem : NetworkBehaviour {
    private const string SCORE_HEADER = "Score: ";
    
    [Networked, SerializeField] private int overallScore { get; set; }
    [SerializeField] private int burnableScore = 100;
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

}
