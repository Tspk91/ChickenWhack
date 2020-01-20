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

    public int timeLimit = 30;
    public int scoreObjective = 10;
    public int chickenAmount = 10;

    public ChickenManager chickenManager;

    public PlayerController playerController;

    public GameplayUI gameplayUI;
    public GameObject gameplayObjects;
    public Camera gameplayCamera;

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
        if (Input.GetKeyDown(KeyCode.Escape))
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

    public void Win()
    {
        if (GameEnded)
            return;

        GameEnded = true;
        onGameEnded(true);
        ApplicationController.ExitGame(GameExitType.WIN, 1f);
    }

    public void Lose()
    {
        if (GameEnded)
            return;

        GameEnded = true;
        onGameEnded(false);
        ApplicationController.ExitGame(GameExitType.LOSE, 2f);
    }

    public void OnPressExit()
    {
        if (GameEnded)
            return;

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
