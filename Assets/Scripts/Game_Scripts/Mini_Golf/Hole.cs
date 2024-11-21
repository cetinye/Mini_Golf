using System;
using UnityEngine;

namespace MiniGolf
{
	public class Hole : Block, IClickable
	{
		public static Action<int, int> FlagPlaced;
		public static bool isFlagSet = false;
		[SerializeField] private GameObject flagPref;

		public void OnClick()
		{
			if (isFlagSet || LevelManager.Instance.GameState != GameState.Playing) return;

			isFlagSet = true;
			Debug.Log("Clicked Hole [" + x + " " + z + "]");
			Instantiate(flagPref, transform);

			FlagPlaced?.Invoke(x, z);
		}
	}
}