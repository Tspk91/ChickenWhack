using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : BaseUI
{
    public Text timeText;
    public Text scoreText;
    public Text objectiveText;

    GameController gameController;

    Coroutine updateTimeCoroutine;
    WaitForSeconds secondWait;

    private void Awake()
    {
        gameController = ApplicationController.refs.gameController;

        gameController.onScored += UpdateScore;

        secondWait = new WaitForSeconds(1f);
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
            timeText.text = string.Format("{0}:{1:00}", seconds / 60, seconds % 60);

            yield return secondWait;
        }
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
