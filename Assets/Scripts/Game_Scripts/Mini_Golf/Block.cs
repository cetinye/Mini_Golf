using UnityEngine;

namespace MiniGolf
{
	public class Block : MonoBehaviour
	{
		public int x;
		public int z;
		public Direction incomingBallDirection;

		public void OnBallVisit()
		{
			Debug.Log("Ball Visited [" + x + " " + z + "]");
		}
	}
}