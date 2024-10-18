using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
	public class GridController : MonoBehaviour
	{
		public static GridController instance;

		[Header("Grid Settings")]
		public int gridWidth = 5;
		public int gridHeight = 5;
		[SerializeField] private float tileSize = 1.0f;

		[Header("Models")]
		[SerializeField] private Ball ball;
		[SerializeField] private Block hole;
		[SerializeField] private List<GameObject> sand = new List<GameObject>();
		[SerializeField] private Obstacle obstacle;
		[SerializeField] private List<GameObject> tree = new List<GameObject>();
		[SerializeField] private Block ballStart;
		[SerializeField] private GameObject pipe;
		[SerializeField] private GameObject flag;
		[SerializeField] private Block flat;

		[Header("Cinemachine")]
		[SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

		private Vector2 ballPosition;
		private Vector2 holePosition;
		private Ball spawnedBall;

		public Block[,] grid;

		void Awake()
		{
			instance = this;
		}

		void Start()
		{
			CreateGrid();
		}

		public void CreateGrid()
		{
			InitializeGrid();
			DisableCorners();
			ChooseBallSpawnPoint();
		}

		private void InitializeGrid()
		{
			grid = new Block[gridWidth, gridHeight];
			float xPos = 0;
			float zPos = 0;

			for (var i = 0; i < gridWidth; i++)
			{
				for (var j = 0; j < gridHeight; j++)
				{
					Block newBlock;
					if (i == 0 || j == 0 || i == gridWidth - 1 || j == gridHeight - 1)
					{
						newBlock = Instantiate(hole, transform);
					}
					else
					{
						newBlock = Instantiate(flat, transform);
					}

					newBlock.x = i;
					newBlock.z = j;

					zPos += tileSize;
					newBlock.transform.position = new Vector3(xPos, 0, zPos);

					grid[i, j] = newBlock;

					targetGroup.AddMember(newBlock.transform, 1.0f, 1.0f);
				}

				xPos += tileSize;
				zPos = 0;
			}

			Block n = grid[5, 3];
			Vector3 pos = n.transform.position;
			Destroy(n.gameObject);
			n = Instantiate(obstacle, transform);
			grid[5, 3] = n;
			n.transform.position = pos;
		}

		private void DisableCorners()
		{
			grid[0, 0].gameObject.SetActive(false);
			grid[0, gridHeight - 1].gameObject.SetActive(false);
			grid[gridWidth - 1, 0].gameObject.SetActive(false);
			grid[gridWidth - 1, gridHeight - 1].gameObject.SetActive(false);
		}

		private void ChooseBallSpawnPoint()
		{
			int rand = Random.Range(0, 4);
			int randIndex;
			Vector3 pos;
			Block block;

			rand = 10;
			block = grid[5, gridHeight - 1];
			SpawnBall(5, gridHeight - 1, block);

			if (rand == 0)
			{
				randIndex = Random.Range(1, gridHeight - 1);
				block = grid[0, randIndex];
				SpawnBall(0, randIndex, block);
			}
			else if (rand == 1)
			{
				randIndex = Random.Range(1, gridHeight - 1);
				block = grid[gridWidth - 1, randIndex];
				SpawnBall(gridWidth - 1, randIndex, block);
			}
			else if (rand == 2)
			{
				randIndex = Random.Range(1, gridWidth - 1);
				block = grid[randIndex, 0];
				SpawnBall(randIndex, 0, block);
			}
			else if (rand == 3)
			{
				randIndex = Random.Range(1, gridWidth - 1);
				block = grid[randIndex, gridHeight - 1];
				SpawnBall(randIndex, gridHeight - 1, block);
			}
		}

		private void SpawnBall(int x, int z, Block block)
		{
			Vector3 pos = block.transform.position;
			Destroy(block.gameObject);

			grid[x, z] = Instantiate(ballStart, transform);
			grid[x, z].transform.position = new Vector3(pos.x, 0f, pos.z);

			spawnedBall = Instantiate(ball);
			spawnedBall.transform.position = new Vector3(grid[x, z].transform.position.x, spawnedBall.transform.position.y, grid[x, z].transform.position.z);

			spawnedBall.x = x;
			spawnedBall.z = z;
		}
	}
}
