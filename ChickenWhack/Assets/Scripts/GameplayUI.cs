using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public Text timeText;
    public Text scoreText;

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
        UpdateScore(0);
        updateTimeCoroutine = StartCoroutine(UpdateTime());
    }

    private void OnDisable()
    {
        StopCoroutine(updateTimeCoroutine);
    }

    private IEnumerator UpdateTime()
    {
        while(true)
        {
            timeText.text = Mathf.CeilToInt(gameController.TimeLeft).ToString();

            yield return secondWait;
        }
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score + "/" + gameController.scoreObjective;
    }
}
