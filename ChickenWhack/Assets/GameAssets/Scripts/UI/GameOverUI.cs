// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : BaseUI
{
	public float hideDuration = 0.5f;
	public float gameOverObjectsScaleDuration = 0.2f;
	public float loseChickenOffset = 2f;
	public float loseChickenWait = 1f;
	public float loseChickenDuration = 1f;

	public GameObject winMsg;
	public GameObject loseMsg;

	public GameObject winBackground;
	public GameObject loseBackground;

	public Transform gameOverObjects;
	public Transform loseChicken;

	public Animator mouseAnimator;

	//ids for faster execution
	int winAnimID = Animator.StringToHash("Win");
	int loseAnimID = Animator.StringToHash("Lose");

	private void Awake()
	{
		winMsg.SetActive(false);
		loseMsg.SetActive(false);
		gameOverObjects.gameObject.SetActive(false);
		loseChicken.gameObject.SetActive(false);
	}

	public void ShowWin()
	{
		this.gameObject.SetActive(true);

		winMsg.SetActive(true);
		loseMsg.SetActive(false);

		winBackground.SetActive(true);
		loseBackground.SetActive(false);

		gameOverObjects.gameObject.SetActive(true);
		StartCoroutine(ShowHideCoroutine(true));

		mouseAnimator.SetTrigger(winAnimID);
	}

	public void ShowLose()
	{
		this.gameObject.SetActive(true);

		winMsg.SetActive(false);
		loseMsg.SetActive(true);

		winBackground.SetActive(false);
		loseBackground.SetActive(true);

		gameOverObjects.gameObject.SetActive(true);
		StartCoroutine(ShowHideCoroutine(true));

		mouseAnimator.SetTrigger(loseAnimID);

		StartCoroutine(ShowLoseChicken());
	}

	public override void Hide()
	{
		base.Hide();

		StartCoroutine(ShowHideCoroutine(false));
	}

	private IEnumerator ShowHideCoroutine(bool show)
	{
		if (show)
		{
			float t = 0f;

			while (t < 1f)
			{
				gameOverObjects.localScale = Vector3.one * t;
				t += Time.deltaTime / gameOverObjectsScaleDuration;

				yield return null;
			}

			gameOverObjects.localScale = Vector3.one;
		}
		else
		{
			yield return new WaitForSecondsRealtime(hideDuration);

			gameOverObjects.gameObject.SetActive(false);

			winMsg.SetActive(false);
			loseMsg.SetActive(false);

			this.gameObject.SetActive(false);
		}
	}

	private IEnumerator ShowLoseChicken()
	{
		yield return new WaitForSecondsRealtime(loseChickenWait);

		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_CHICKENFLY);

		loseChicken.gameObject.SetActive(true);

		float t = 0f;

		Vector3 chickenPos = loseChicken.localPosition;

		while (t < 1f)
		{
			chickenPos.x = Mathf.Lerp(-loseChickenOffset, loseChickenOffset, t);
			loseChicken.localPosition = chickenPos;
			t += Time.deltaTime / loseChickenDuration;

			yield return null;
		}

		loseChicken.gameObject.SetActive(false);
	}
}
