using Lean.Touch;
using UnityEngine;

namespace GrandTour
{
	public class TapListener : MonoBehaviour
	{
		void OnEnable()
		{
			LeanTouch.OnFingerDown += OnFingerDown;
		}

		void OnDisable()
		{
			LeanTouch.OnFingerDown -= OnFingerDown;
		}

		private void OnFingerDown(LeanFinger finger)
		{
			Physics.Raycast(finger.GetRay(), out RaycastHit hit);
			if (hit.collider != null && hit.collider.TryGetComponent(out IClickable clickable))
			{
				clickable.OnClick();
			}
		}
	}
}