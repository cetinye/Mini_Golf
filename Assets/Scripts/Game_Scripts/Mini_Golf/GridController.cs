using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
	public class GridController : MonoBehaviour
	{
		public static GridController instance;

		[Header("Grid Settings")]
		public int gridWidth;
		public int gridHeight;
		public Block[,] grid;
		[SerializeField] private List<Block> path = new List<Block>();
		[SerializeField] private int obstacleCount;
		[SerializeField] private int pipeCount;
		[SerializeField] private float tileSize = 1.0f;
		[SerializeField] private List<Color> pipeColors = new List<Color>();
		private Ball spawnedBall;
		private int lastIndex;
		private List<Block> destroyList = new List<Block>();

		[Header("Models")]
		[SerializeField] private Ball ball;
		[SerializeField] private Block hole;
		[SerializeField] private List<GameObject> sand = new List<GameObject>();
		[SerializeField] private Obstacle obstacle;
		[SerializeField] private List<GameObject> tree = new List<GameObject>();
		[SerializeField] private Block ballStart;
		[SerializeField] private Pipe pipe;
		[SerializeField] private GameObject flag;
		[SerializeField] private Block flat;

		[Header("Cinemachine")]
		[SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

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
			ChooseBallSpawnPoint();
			CalculateBallPath();
			SpawnObstacles(obstacleCount);
			SpawnPipes(pipeCount);
			DisableCorners();
		}

		public void DeleteGrid()
		{
			for (var i = 0; i < gridWidth; i++)
			{
				for (var j = 0; j < gridHeight; j++)
				{
					targetGroup.RemoveMember(grid[i, j].transform);
					Destroy(grid[i, j].gameObject);
				}
			}

			for (int i = 0; i < transform.childCount; i++)
			{
				Destroy(transform.GetChild(i).gameObject);
			}

			for (int i = 0; i < destroyList.Count; i++)
			{
				if (destroyList[i] == null) continue;

				Destroy(destroyList[i].gameObject);
			}

			destroyList.Clear();
			Hole.isFlagSet = false;
			grid = null;
			path.Clear();
			lastIndex = 0;
			Destroy(spawnedBall.gameObject);

			CreateGrid();
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
		}

		private void SpawnObstacles(int obstacleCount)
		{
			for (int i = 0; i < obstacleCount; i++)
			{
				Block b = GetPathBlock();
				destroyList.Add(b);

				// if not available then delete grid
				if (!b.gameObject.activeSelf)
				{
					DeleteGrid();
					return;
				}

				SetObstacle(b.x, b.z, (Rotation)Random.Range(0, 2));
				CalculateBallPath();
			}
		}

		private void SpawnPipes(int pipeCount)
		{
			for (int i = 0; i < pipeCount; i++)
			{
				Block b1 = GetPathBlock();
				destroyList.Add(b1);

				// if not available then delete grid
				if (!b1.gameObject.activeSelf)
				{
					DeleteGrid();
					return;
				}

				Pipe p1 = SetPipe(b1.x, b1.z, GetOppositeDirection(b1.incomingBallDirection), pipeColors[i]);
				destroyList.Add(p1);

				Block b2 = GetPathBlock();
				destroyList.Add(b2);

				// if not available then delete grid
				if (!b2.gameObject.activeSelf)
				{
					DeleteGrid();
					return;
				}

				Pipe p2 = SetPipe(b2.x, b2.z, (Direction)Random.Range(0, 4), pipeColors[i]);
				destroyList.Add(p2);

				ConnectPipes(p1, p2);

				CalculateBallPath();
			}
		}

		private Block GetRandomBlock()
		{
			int x;
			int z;

			do
			{
				x = Random.Range(1, gridWidth - 2);
				z = Random.Range(1, gridHeight - 2);
			} while (grid[x, z].TryGetComponent(out Obstacle o) || grid[x, z].TryGetComponent(out Pipe p));

			return grid[x, z];
		}

		private Block GetPathBlock()
		{
			int randIdx;
			int tries = 0;
			do
			{
				if (++tries > 10 || lastIndex + 1 >= path.Count)
				{
					GameObject g = new GameObject();
					g.AddComponent<Block>();
					g.SetActive(false);
					return g.GetComponent<Block>();
				}

				randIdx = Random.Range(lastIndex + 1, path.Count);

			} while (path[randIdx].TryGetComponent(out Obstacle o) || path[randIdx].TryGetComponent(out Pipe p)
			|| path[randIdx].x <= 0 || path[randIdx].z <= 0 || path[randIdx].x >= gridWidth - 1 || path[randIdx].z >= gridHeight - 1);

			lastIndex = randIdx;
			return path[randIdx];
		}

		private void CalculateBallPath()
		{
			int tries = 0;
			path.Clear();

			Ball tempBall = Instantiate(ball, transform);
			tempBall.SetMeshRendererState(false);
			tempBall.x = spawnedBall.x;
			tempBall.z = spawnedBall.z;
			tempBall.SetGridController(this);
			tempBall.DecideOnDirection();

			do
			{
				grid[tempBall.x, tempBall.z].incomingBallDirection = tempBall.direction;
				path.Add(grid[tempBall.x, tempBall.z]);

				// Move ball in the chosen direction
				switch (tempBall.direction)
				{
					case Direction.UP:
						tempBall.z++;
						break;
					case Direction.DOWN:
						tempBall.z--;
						break;
					case Direction.LEFT:
						tempBall.x--;
						break;
					case Direction.RIGHT:
						tempBall.x++;
						break;
				}

				// break if OOB
				if (tempBall.x < 0 || tempBall.z < 0 || tempBall.x >= gridWidth || tempBall.z >= gridHeight)
					break;

				if (grid[tempBall.x, tempBall.z].TryGetComponent(out Obstacle o))
				{
					tempBall.direction = o.GetDirection(tempBall.direction);
					continue;
				}

				if (grid[tempBall.x, tempBall.z].TryGetComponent(out Pipe p))
				{
					if (p.enterDirection == tempBall.direction)
					{
						p.BallEnter(tempBall);
						continue;
					}
				}

				if (++tries >= 100 || (tempBall.x == spawnedBall.x && tempBall.z == spawnedBall.z))
				{
					Debug.LogError("Failed to find path");
					DeleteGrid();
					return;
				}

			} while (tempBall.x >= 0 && tempBall.z >= 0 && tempBall.x < gridWidth && tempBall.z < gridHeight);

			Debug.Log("Path ends at: [" + path[^1].x + " " + path[^1].z + "]");
			Destroy(tempBall.gameObject);
		}

		private void SetObstacle(int x, int z, Rotation rot)
		{
			// Get and delete existing block
			Block block = grid[x, z];
			Vector3 pos = block.transform.position;
			Destroy(block.gameObject);

			// Replace with obstacle
			Obstacle o = Instantiate(obstacle, transform);
			grid[x, z] = o;
			o.x = x;
			o.z = z;
			o.transform.position = pos;
			o.SetRotation(rot);
		}

		private Pipe SetPipe(int x, int z, Direction dir, Color color)
		{
			// Get and delete existing block
			Block block = grid[x, z];
			Vector3 pos = block.transform.position;
			Destroy(block.gameObject);

			// Replace with pipe
			Pipe p = Instantiate(pipe, transform);
			grid[x, z] = p;
			p.x = x;
			p.z = z;
			p.transform.position = pos;
			p.SetLookingDirection(dir);
			p.SetColor(color);

			return p;
		}

		private void ConnectPipes(Pipe p1, Pipe p2)
		{
			p1.SetExitPipe(p2);
			p2.SetExitPipe(p1);
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

			spawnedBall.SetGridController(this);
		}

		private Direction GetOppositeDirection(Direction d)
		{
			if (d == Direction.UP)
				return Direction.DOWN;
			if (d == Direction.DOWN)
				return Direction.UP;
			if (d == Direction.LEFT)
				return Direction.RIGHT;
			if (d == Direction.RIGHT)
				return Direction.LEFT;

			return d;
		}
	}
}