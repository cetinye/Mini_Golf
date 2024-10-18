using UnityEngine;

namespace GrandTour
{
	public class Hole : Block, IClickable
	{
		public static bool isFlagSet = false;
		[SerializeField] private GameObject flagPref;

		public void OnClick()
		{
			if (isFlagSet) return;

			isFlagSet = true;
			Instantiate(flagPref, transform);
		}
	}
}