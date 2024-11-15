using System;
using UnityEngine;

namespace MiniGolf
{
	public class Hole : Block, IClickable
	{
		public static Action FlagPlaced;
		public static bool isFlagSet = false;
		[SerializeField] private GameObject flagPref;

		public void OnClick()
		{
			if (isFlagSet) return;

			isFlagSet = true;
			Debug.Log("Clicked Hole [" + x + " " + z + "]");
			Instantiate(flagPref, transform);

			FlagPlaced?.Invoke();
		}
	}
}