using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool gameEnded = false;

    public float TimeLeft { get { return timeLimit - (Time.time - startTimeStamp); } }
    private float startTimeStamp;

    public int Score { get; private set; }

    public event System.Action<int> onScored = delegate { };

    private void Awake()
    {
        SetObjectsActive(false);
    }

    public void StartGameplay()
    {
        startTimeStamp = Time.time;

        gameEnded = false;
        enabled = true;

        SetObjectsActive(true);

        chickenManager.SpawnChickens(chickenAmount);
        chickenManager.onChickenWhacked += OnChickenWhacked;

        loseCoroutine = this.DelayedAction(Lose, timeLimit);
    }

    public void StopGameplay()
    {
        enabled = false;

        Score = 0;

        SetObjectsActive(false);

        chickenManager.Clear();
        chickenManager.onChickenWhacked -= OnChickenWhacked;

        this.CancelAction(loseCoroutine);
    }

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
        if (gameEnded)
            return;

        gameEnded = true;
        ApplicationController.ExitGame(ExitType.WIN);
    }

    public void Lose()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        ApplicationController.ExitGame(ExitType.LOSE);
    }

    public void OnPressExit()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        ApplicationController.ExitGame(ExitType.MENU);

        gameplayUI.Hide();
    }

    private void OnChickenWhacked()
    {
        if (gameEnded)
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
