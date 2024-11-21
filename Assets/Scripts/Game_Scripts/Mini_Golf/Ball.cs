using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace MiniGolf
{
	public class Ball : MonoBehaviour
	{
		public int x;
		public int z;

		public Direction direction = Direction.UP;
		public float moveDuration;
		public float fadeDuration;

		[SerializeField] private Transform golfStick;

		private MeshRenderer meshRenderer;
		private GridController gridController;
		private Block clickedHole;

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

		void OnFlagPlaced(int x, int z)
		{
			if (LevelManager.Instance.GameState != GameState.Playing) return;

			LevelManager.Instance.GameState = GameState.Selected;
			clickedHole = gridController.grid[x, z];

			Invoke(nameof(StartMove), 0.5f);
		}

		private void StartMove()
		{
			StartCoroutine(GolfClubRoutine());
		}

		private IEnumerator GolfClubRoutine()
		{
			int ballRotationY = 0;
			Transform golfClub = Instantiate(golfStick, transform);

			if (x == 0 && z != 0)
			{
				ballRotationY = 0;
			}
			else if (x == gridController.gridWidth - 1 && z != 0)
			{
				ballRotationY = 180;
			}
			else if (z == 0 && x != 0)
			{
				ballRotationY = 270;
			}
			else if (z == gridController.gridHeight - 1 && x != 0)
			{
				ballRotationY = 90;
			}

			transform.DOLocalRotate(new Vector3(transform.rotation.x, ballRotationY, transform.rotation.z), 0f);

			golfClub.DOLocalRotate(new Vector3(0, 0, 0), 0f);
			yield return new WaitForEndOfFrame();
			golfClub.DOLocalRotate(new Vector3(0, 0, -37), 0.75f);
			yield return new WaitForSeconds(0.75f);
			golfClub.DOLocalRotate(new Vector3(0, 0, 6), 0.125f);
			yield return new WaitForSeconds(0.125f);
			golfClub.gameObject.SetActive(false);
			yield return Move();
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

				if (LevelManager.Instance.GetFinishBlock() == clickedHole)
				{
					LevelManager.Instance.GameState = GameState.Success;
					LevelManager.Instance.RoundComplete(true);
				}
				else
				{
					LevelManager.Instance.GameState = GameState.Fail;
					LevelManager.Instance.RoundComplete(false);
				}

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
				SetMeshRendererState(false, true);
				SetBallPosition(x, z);
				yield return new WaitForSeconds(1f);
				SetMeshRendererState(true);
			}

			// Recursively call Move
			yield return Move();
		}

		public void SetMeshRendererState(bool state, bool isInstant = false)
		{
			meshRenderer.enabled = false;

			if (isInstant)
				meshRenderer.enabled = state;
			else
			{
				if (state)
				{
					FadeBall(0, 0f);
					meshRenderer.enabled = true;
					FadeBall(1, fadeDuration).OnComplete(() =>
					{
						if (LevelManager.Instance.GameState == GameState.Preview)
						{
							LevelManager.Instance.GameState = GameState.Playing;
						}
					});
				}
				else
				{
					FadeBall(1, 0f);
					meshRenderer.enabled = true;
					FadeBall(0, fadeDuration);
				}
			}
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