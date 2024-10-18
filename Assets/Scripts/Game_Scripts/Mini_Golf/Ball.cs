using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace GrandTour
{
	public class Ball : MonoBehaviour
	{
		public int x;
		public int z;

		public Direction direction = Direction.UP;
		public float moveDuration;
		public float fadeDuration;

		private MeshRenderer meshRenderer;
		private GridController gridController;

		private void Awake()
		{
			meshRenderer = GetComponentInChildren<MeshRenderer>();
		}

		public void SetGridController(GridController gridController)
		{
			this.gridController = gridController;
		}

		void OnEnable()
		{
			Hole.FlagPlaced += OnFlagPlaced;
		}

		void OnDisable()
		{
			Hole.FlagPlaced -= OnFlagPlaced;
		}

		void OnFlagPlaced()
		{
			StartCoroutine(Move());
		}

		private IEnumerator Move()
		{
			DecideOnDirection();

			Vector3 targetPosition = new Vector3(
				gridController.grid[x, z].transform.position.x,
				transform.position.y,
				gridController.grid[x, z].transform.position.z
			);

			// Move the ball using DOTween
			Tween movementTween = transform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear);
			yield return movementTween.WaitForCompletion();

			gridController.grid[x, z].OnBallVisit();

			if (x == 0 || z == 0 || x == gridController.gridWidth - 1 || z == gridController.gridHeight - 1)
			{
				Debug.Log("Ball reached end of grid");
				Tween t = transform.DOMoveY(0, 0.5f);
				yield return t.WaitForCompletion();
				yield break;
			}

			// Check for obstacles or pipes
			if (gridController.grid[x, z].TryGetComponent(out Obstacle obstacle))
			{
				direction = obstacle.GetDirection(direction);
			}
			else if (gridController.grid[x, z].TryGetComponent(out Pipe pipe) && pipe.enterDirection == direction)
			{
				pipe.BallEnter(this);

				// Fade the ball out and in
				FadeBall(0, fadeDuration);
				SetMeshRendererState(false);
				SetBallPosition(x, z);
				yield return new WaitForSeconds(1f);
				SetMeshRendererState(true);
				FadeBall(1, fadeDuration);
			}

			// Recursively call Move
			yield return Move();
		}

		public void SetMeshRendererState(bool state)
		{
			meshRenderer.enabled = state;
		}

		public Tween FadeBall(float val, float duration)
		{
			return meshRenderer.materials[0].DOColor(new Color(1, 1, 1, val), duration);
		}

		public void DecideOnDirection()
		{
			// Determine the direction based on ball position within grid bounds
			if (x == 0 && z != 0)
				direction = Direction.RIGHT;
			else if (z == 0 && x != 0)
				direction = Direction.UP;
			else if (x == gridController.gridWidth - 1 && z != gridController.gridHeight - 1)
				direction = Direction.LEFT;
			else if (z == gridController.gridHeight - 1 && x != gridController.gridWidth - 1)
				direction = Direction.DOWN;

			// Move ball in the chosen direction
			switch (direction)
			{
				case Direction.UP:
					z++;
					break;
				case Direction.DOWN:
					z--;
					break;
				case Direction.LEFT:
					x--;
					break;
				case Direction.RIGHT:
					x++;
					break;
			}
		}

		public void SetBallPosition(int x, int z)
		{
			transform.position = new Vector3(
				gridController.grid[x, z].transform.position.x,
				transform.position.y,
				gridController.grid[x, z].transform.position.z
			);
		}
	}

	public enum Direction
	{
		UP,
		DOWN,
		LEFT,
		RIGHT
	}
}