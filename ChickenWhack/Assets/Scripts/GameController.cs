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

    public GameObject gameplayUI;
    public GameObject gameplayObjects;

    private uint loseCoroutine;

    public int Score { get; private set; }

    public event System.Action onScored = delegate { };

    public void StartGameplay()
    {
        enabled = true;

        gameplayUI.SetActive(true);
        gameplayObjects.SetActive(true);
        playerController.gameObject.SetActive(true);

        chickenManager.SpawnChickens(chickenAmount);
        chickenManager.onChickenWhacked += OnChickenWhacked;

        loseCoroutine = this.DelayedAction(Lose, timeLimit);
    }

    public void StopGameplay()
    {
        enabled = false;

        Score = 0;

        gameplayUI.SetActive(false);
        gameplayObjects.SetActive(false);
        playerController.gameObject.SetActive(false);

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

    public void Win()
    {
        ApplicationController.ExitGame(ExitType.WIN);
    }

    public void Lose()
    {
        ApplicationController.ExitGame(ExitType.LOSE);
    }

    public void OnPressExit()
    {
        ApplicationController.ExitGame(ExitType.MENU);
    }

    private void OnChickenWhacked()
    {
        Score += 1;
        onScored();

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
