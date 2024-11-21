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
		public MeshRenderer meshRenderer;
		[SerializeField] private Material _opaqueMaterial;
		private Material _transparentMaterial;
		public bool IsPipe;
		private Material[] mats;
		[SerializeField] private VisibilityController visibilityController;

		void Start()
		{
			if (meshRenderer != null && _opaqueMaterial != null)
			{
				_transparentMaterial = meshRenderer.materials[0];
				mats = null;
				mats = new Material[1];
				mats[0] = Instantiate(_opaqueMaterial);
			}
		}

		public void OnBallVisit()
		{
			Debug.Log("Ball Visited [" + x + " " + z + "]");
		}

		public void HideShowControl(bool state)
		{
			visibilityController.SetVisible(state);
		}

		public void SetMeshRendererState(bool state, bool isInstant = false)
		{
			if (meshRenderer == null)
			{
				return;
			}

			meshRenderer.materials[0] = _transparentMaterial;
			Material _material = meshRenderer.materials[0];
			meshRenderer.enabled = false;

			if (isInstant)
			{
				meshRenderer.enabled = state;
			}
			else
			{
				if (state)
				{
					_material.DOFade(0, 0f);
					meshRenderer.enabled = true;
					_material.DOFade(1, fadeTime).OnComplete(() =>
					{
						if (IsPipe)
						{
							mats[0].color = _material.color;
							meshRenderer.materials = mats;
						}
					});
				}
				else
				{
					_material.DOFade(1, 0f);
					meshRenderer.enabled = true;
					_material.DOFade(0, fadeTime).OnComplete(() =>
					{
						meshRenderer.enabled = false;
					});
				}
			}
		}
	}
}