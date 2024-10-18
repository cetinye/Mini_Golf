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

		private GridController gridController;

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

			yield return Move();
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