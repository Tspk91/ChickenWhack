// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Has the serialized game settings.
/// Controls the gameplay phase of the app
/// -Lose condition countdown
/// -Player scoring
/// -Game over...
/// </summary>
public class GameController : MonoBehaviour
{
    public float gameAreaRadius = 40f;

	public int minChickenAmount = 16;
	public int maxChickenAmount = 64;
	public int baseTimeLimit = 30;
	public int timeLimitPerMinChickenAmount = 30;

    public int timeLimit = 30;
    public int scoreObjective = 10;
    public int chickenAmount = 10;

    public ChickenManager chickenManager;

    public PlayerController playerController;

	public GameOverUI gameOverUI;
    public GameplayUI gameplayUI;
    public Camera gameplayCamera;
    public GameObject gameplayObjects;
    public GameObject portraitScreenScenery;
	public GameObject landscapeScreenScenery;

    private uint loseCoroutine;

    /// <summary>
    /// Is the game over? (does not imply we are outside the gameplay phase
    /// </summary>
    public bool GameEnded { get; private set; }

    /// <summary>
    /// Time left to lose
    /// </summary>
    public float TimeLeft { get { return timeLimit - (Time.time - startTimeStamp); } }
    private float startTimeStamp;

    /// <summary>
    /// Current player score
    /// </summary>
    public int Score { get; private set; }

    public event System.Action<bool> onGameEnded = delegate { };
    public event System.Action<int> onScored = delegate { };

    private void Awake()
    {
        SetObjectsActive(false);

		if (!ApplicationController.refs.AR_controller.AR_Enabled)
		{
			portraitScreenScenery.SetActive(true);
			gameplayCamera.GetComponent<HorizontalFovLocker>().onLandscapeShowStart += () => { landscapeScreenScenery.SetActive(true); };
			gameplayCamera.GetComponent<HorizontalFovLocker>().onLandscapeHideEnd += () => { landscapeScreenScenery.SetActive(false); };
		}
		else
		{
			portraitScreenScenery.SetActive(false);
			landscapeScreenScenery.SetActive(false);
		}
    }

    //Activates the gameplay phase
    public void StartGameplay()
    {
        startTimeStamp = Time.time;

        GameEnded = false;
        enabled = true;

        SetObjectsActive(true);

        chickenManager.SpawnChickens(chickenAmount);
        chickenManager.onChickenWhacked += OnChickenWhacked;

        loseCoroutine = this.DelayedAction(Lose, timeLimit);

		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_GAMEMUSIC);
	}

	//Deactivates the gameplay phase
	public void StopGameplay()
    {
        enabled = false;

        Score = 0;

        SetObjectsActive(false);

        chickenManager.Clear();
        chickenManager.onChickenWhacked -= OnChickenWhacked;

        this.CancelAction(loseCoroutine);
    }

    //Checks for input to return to menu
    private void Update()
    {
        if (InputController.GetEscapeDown())
        {
            OnPressExit();
        }
    }

    private void SetObjectsActive(bool state)
    {
        gameplayUI.gameObject.SetActive(state);
        gameplayObjects.SetActive(state);
        gameplayCamera.gameObject.SetActive(state);
    }

	public void SetChickenAmount(int amount)
	{
		chickenAmount = amount;
		scoreObjective = amount;
		timeLimit = baseTimeLimit + (int)(((float)amount / minChickenAmount) * timeLimitPerMinChickenAmount);
	}

    public void Win()
    {
        if (GameEnded)
            return;

        GameEnded = true;
        onGameEnded(true);
        ApplicationController.ExitGame(GameExitType.WIN, 5f);

		ApplicationController.refs.audioController.PlayEvent(AudioEvent.STOP_MUSIC);
		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_VICTORY);

		this.DelayedAction(gameplayUI.Hide, 0.5f);

		this.DelayedAction(gameOverUI.ShowWin, 0.75f);
		this.DelayedAction(gameOverUI.Hide, 5f);
	}

	public void Lose()
    {
        if (GameEnded)
            return;

        GameEnded = true;
        onGameEnded(false);
        ApplicationController.ExitGame(GameExitType.LOSE, 5f);

		ApplicationController.refs.audioController.PlayEvent(AudioEvent.STOP_MUSIC);
		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_TIMEUP);

		this.DelayedAction(gameplayUI.Hide, 1.5f);

		this.DelayedAction(gameOverUI.ShowLose, 1.75f);
		this.DelayedAction(gameOverUI.Hide, 5f);
	}

	public void OnPressExit()
    {
        if (GameEnded)
            return;

		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_BUTTON);

		GameEnded = true;
        ApplicationController.ExitGame(GameExitType.MENU, 0f);

        gameplayUI.Hide();
    }

    /// <summary>
    /// Increases score when whacking a chicken if game is not over, checks for victory
    /// </summary>
    private void OnChickenWhacked()
    {
        if (GameEnded)
            return;

        Score += 1;
        onScored(Score);

        if (Score == scoreObjective)
        {
            Win();
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, gameAreaRadius);
#endif
    }
}
