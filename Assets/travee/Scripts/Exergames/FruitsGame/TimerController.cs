using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace FruitsGame
{
    public class TimerController : MonoBehaviour
    {
        public Image timeImage;
        public TMP_Text timeText;

        protected InputData _inputData;
        protected UnityEvent _onTimeFinished;

        private bool _timer;
        private bool _pause;

        private float _gameMaxDuration;
        private float _gameDuration;

        private IEnumerator _coroutine;

        public bool Pause
        {
            get { return _pause; }
            set { _pause = value; }
        }

        public void Init(InputData inputData, UnityAction onTimeFinishedAction)
        {
            _inputData = inputData;

            _onTimeFinished = new UnityEvent();
            _onTimeFinished.AddListener(onTimeFinishedAction);
        }

        public void UpdateTime(InputData inputData)
        {
            _inputData = inputData;
        }

        public void StartTime()
        {
            _gameMaxDuration = (int)_inputData.CountdownTimer * 60;

            _pause = false;

            if (_inputData.CountdownTimer > 0) {
                _timer = true;
            }

            if (_inputData.CountdownTimer == 0) {
                _timer = false;
            }

            if (_timer) {
                _coroutine = Timer();

                StartCoroutine(_coroutine);
            }

            if (!_timer) {
                _coroutine = StopWatch();

                StartCoroutine(_coroutine);
            }
        }

        public void StopTime()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator Timer()
        {
            _gameDuration = 0;

            int tempTime = (int) _gameMaxDuration;
            timeText.text = _gameMaxDuration.ToString();
            while (tempTime > 0) {
                yield return new WaitForSeconds(1);

                if (!_pause) {
                    _gameDuration ++;

                    tempTime--;
                    timeImage.fillAmount = tempTime / (float)_gameMaxDuration;
                    timeText.text = tempTime.ToString();
                }
            }

            _onTimeFinished.Invoke();
            //GameManager.Instance.OnTimeFinished();
            //Game over
            //CardGameManager.instance.GameOver();

        }

        IEnumerator StopWatch()
        {
            _gameDuration = 0;

            timeText.text = _gameDuration.ToString();
            timeImage.fillAmount = 1;

            while (true) {
                yield return new WaitForSeconds(1);

                if (!_pause) {
                    _gameDuration++;
                    timeText.text = _gameDuration.ToString();
                }
            }
        }

        public void StopTimer()
        {
            StopCoroutine("Timer");
        }
    }
}
