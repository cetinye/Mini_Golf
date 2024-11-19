using DG.Tweening;
using UnityEngine;

namespace MiniGolf
{
	public class Block : MonoBehaviour
	{
		public int x;
		public int z;
		public Direction incomingBallDirection;
		[SerializeField] private float fadeTime;
		[SerializeField] private MeshRenderer _meshRenderer;
		[SerializeField] private Material _material;

		public void OnBallVisit()
		{
			Debug.Log("Ball Visited [" + x + " " + z + "]");
		}

		public void SetMeshRendererState(bool state, bool isInstant = false)
		{
			if (_meshRenderer == null) return;

			_meshRenderer.enabled = false;

			if (isInstant)
				_meshRenderer.enabled = state;
			else
			{
				if (state)
				{
					_material.DOFade(0, 0f);
					_meshRenderer.enabled = true;
					_material.DOFade(1, fadeTime);
				}
				else
				{
					_material.DOFade(1, 0f);
					_meshRenderer.enabled = true;
					_material.DOFade(0, fadeTime).OnComplete(() => _meshRenderer.enabled = false);
				}
			}
		}
	}
}