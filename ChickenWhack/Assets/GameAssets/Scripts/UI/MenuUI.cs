// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : BaseUI
{
	public UnityEngine.UI.Slider chickenSlider;
	public UnityEngine.UI.Text chickenNumber;
	public UnityEngine.UI.Image[] extraChickenIcons;

	private void Start()
	{
		chickenSlider.minValue = ApplicationController.refs.gameController.minChickenAmount;
		chickenSlider.maxValue = ApplicationController.refs.gameController.maxChickenAmount;
		chickenSlider.value = chickenSlider.minValue;

		OnChickenSliderChange();
	}

	public void OnChickenSliderChange()
	{
		int amount = (int)chickenSlider.value;

		chickenNumber.text = amount.ToString();

		float chickenFraction = 1f - (float)(amount - chickenSlider.minValue) / (chickenSlider.maxValue - chickenSlider.minValue);
		for (int i = 0; i < extraChickenIcons.Length; i++)
		{
			float fraction = (float)i / extraChickenIcons.Length;
			extraChickenIcons[i].enabled = fraction >= chickenFraction;
		}

		ApplicationController.refs.gameController.SetChickenAmount(amount);
	}
}
