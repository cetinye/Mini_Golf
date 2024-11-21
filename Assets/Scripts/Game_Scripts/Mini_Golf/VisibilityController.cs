using System.Collections.Generic;
using UnityEngine;

namespace MiniGolf
{
	public class VisibilityController : MonoBehaviour
	{
		[SerializeField] private List<GameObject> objects = new List<GameObject>();

		public void SetVisible(bool visible)
		{
			foreach (var obj in objects)
			{
				obj.SetActive(visible);
			}
		}
	}
}