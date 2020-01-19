using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : BaseUI
{
    public int dangerTime = 10;
    public Text timeText;
    public Text scoreText;
    public Text objectiveText;
    public Animation textParent;

    GameController gameController;

    Coroutine updateTimeCoroutine;
    WaitForSeconds secondWait;

    Color timeTextColor;

    private void Awake()
    {
        gameController = ApplicationController.refs.gameController;

        gameController.onScored += UpdateScore;

        secondWait = new WaitForSeconds(1f);

        timeTextColor = timeText.color;
    }

    private void OnEnable()
    {
        objectiveText.text = gameController.scoreObjective.ToString();

        UpdateScore(0);
        updateTimeCoroutine = StartCoroutine(UpdateTime());
    }

    public override void Hide()
    {
        base.Hide();

        StopCoroutine(updateTimeCoroutine);
    }

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            int seconds = Mathf.CeilToInt(gameController.TimeLeft);

            //Danger color animation
            if (seconds <= dangerTime)
            {
                timeText.CrossFadeColor(Color.red, 0f, true, false);

                if (seconds < 0)
                    yield break;

                timeText.CrossFadeColor(timeTextColor, 1f, true, false);
            }

            timeText.text = string.Format("{0}:{1:00}", seconds / 60, seconds % 60);

            yield return secondWait;
        }
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();

        textParent.Play();
    }
}
