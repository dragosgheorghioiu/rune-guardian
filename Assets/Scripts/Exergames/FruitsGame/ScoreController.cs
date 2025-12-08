using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FruitsGame
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreText;

        private int _score;

        public int Score
        {
            get { return _score; }
        }

        public void Init()
        {
            AddScore(0);
        }

        public void StartGame()
        {
            _score = 0;

            scoreText.text = _score.ToString("D4");
        }

        public void AddScore(int scoreToAdd)
        {
            _score += scoreToAdd;
            scoreText.text = _score.ToString("D4");
        }
    }
}
