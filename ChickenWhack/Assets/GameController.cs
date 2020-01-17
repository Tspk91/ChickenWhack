using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int timeLimit = 30;
    public int chickenAmount = 10;

    public ChickenManager chickenManager;

    public PlayerController playerController;

    public GameObject gameplayUI;
    public GameObject gameplayObjects;

    private Coroutine loseCoroutine;

    public void StartGameplay()
    {
        enabled = true;

        gameplayUI.SetActive(true);
        gameplayObjects.SetActive(true);
        playerController.gameObject.SetActive(true);

        chickenManager.SpawnChickens(chickenAmount);

        loseCoroutine = ApplicationController.Invoke(Lose, timeLimit);
    }

    public void StopGameplay()
    {
        enabled = false;

        gameplayUI.SetActive(false);
        gameplayObjects.SetActive(false);
        playerController.gameObject.SetActive(false);

        chickenManager.Clear();

        ApplicationController.CancelInvoke(loseCoroutine);
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
}
