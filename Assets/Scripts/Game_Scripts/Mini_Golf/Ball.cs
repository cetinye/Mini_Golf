using System;
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
		public float moveTime;
		public float timeToFade;

		private MeshRenderer meshRenderer;
		private GridController gridController;

		void Awake()
		{
			meshRenderer = GetComponentInChildren<MeshRenderer>();
		}

		void Start()
		{
			gridController = GridController.instance;
			StartCoroutine(Move());
		}

		IEnumerator Move()
		{
			if (x == 0 && z != 0)
				direction = Direction.RIGHT;
			else if (x != 0 && z == 0)
				direction = Direction.UP;
			else if (x == gridController.gridWidth - 1 && z != gridController.gridHeight - 1)
				direction = Direction.LEFT;
			else if (x != gridController.gridWidth - 1 && z == gridController.gridHeight - 1)
				direction = Direction.DOWN;

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
			Vector3 pos = new Vector3(gridController.grid[x, z].transform.position.x, transform.position.y, gridController.grid[x, z].transform.position.z);
			Tween t = transform.DOMove(pos, moveTime).SetEase(Ease.Linear);
			yield return t.WaitForCompletion();

			gridController.grid[x, z].OnBallVisit();

			if (gridController.grid[x, z].TryGetComponent(out Obstacle obstacle))
			{
				direction = obstacle.GetDirection(direction);
			}
			else if (gridController.grid[x, z].TryGetComponent(out Pipe pipe))
			{
				if (pipe.enterDirection == direction)
				{
					pipe.BallEnter(this);
					meshRenderer.materials[0].DOColor(new Color(1, 1, 1, 0), timeToFade);
					yield return new WaitForSeconds(timeToFade);
					SetBallPosition(x, z);
					yield return new WaitForSeconds(1f);
					meshRenderer.materials[0].DOColor(new Color(1, 1, 1, 1), timeToFade);
					yield return new WaitForSeconds(timeToFade);
				}
			}

			yield return Move();
		}

		public void SetBallPosition(int x, int z)
		{
			Vector3 pos = new Vector3(gridController.grid[x, z].transform.position.x, transform.position.y, gridController.grid[x, z].transform.position.z);
			transform.position = pos;
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