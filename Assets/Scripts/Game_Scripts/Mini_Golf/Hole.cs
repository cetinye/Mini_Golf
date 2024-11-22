using System;
using UnityEngine;
using DG.Tweening;

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
			AudioManager.Instance.PlayOneShot(SoundType.Flag);
			GameObject flag = Instantiate(flagPref, transform);
			float startPos = flag.transform.localPosition.y;
			flag.transform.localPosition = new Vector3(flag.transform.localPosition.x, transform.localPosition.y + 5, flag.transform.localPosition.z);
			flag.transform.DOLocalMoveY(startPos, 0.25f).OnComplete(() => FlagPlaced?.Invoke(x, z));
		}
	}
}