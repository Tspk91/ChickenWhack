// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : BaseUI
{
    /// <summary>
    /// Time remaining when timer starts flashing
    /// </summary>
    public int dangerTime = 10;

    public Text timeText;
    public Text scoreText;
    public Text objectiveText;
    public Animation textParent;

    GameController gameController;

    Coroutine updateTimeCoroutine;
    WaitForSeconds secondWait; //avoid gc

    Color timeTextColor;

	bool playingDangerSound;

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

		if (playingDangerSound)
		{
			ApplicationController.refs.audioController.PlayEvent(AudioEvent.STOP_TIMEDANGER);
			playingDangerSound = false;
		}
	}

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            int seconds = Mathf.CeilToInt(gameController.TimeLeft);

            //Danger color animation
            if (seconds <= dangerTime)
            {
				if (!playingDangerSound)
				{
					ApplicationController.refs.audioController.PlayEvent(AudioEvent.START_TIMEDANGER);
					playingDangerSound = true;
				}

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
