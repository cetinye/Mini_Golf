using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiniGolf
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
		private List<GameObject> destroyList = new List<GameObject>();
		private int pipeColorIdx = 0;

		[Header("Models")]
		[SerializeField] private Ball ball;
		[SerializeField] private Block hole;
		[SerializeField] private Obstacle obstacle;
		[SerializeField] private List<GameObject> decorations = new List<GameObject>();
		[SerializeField] private Block ballStart;
		[SerializeField] private Pipe pipe;
		[SerializeField] private GameObject flag;
		[SerializeField] private Block flat;

		[Header("Cinemachine")]
		[SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Destroy(gameObject);
				instance = null;
			}
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
			SpawnDecorations();
		}

		public void DeleteGrid()
		{
			for (var i = 0; i < gridWidth; i++)
			{
				for (var j = 0; j < gridHeight; j++)
				{
					if (grid[i, j] != null)
					{
						targetGroup.RemoveMember(grid[i, j].transform);
						Destroy(grid[i, j].gameObject);
					}
				}
			}

			for (int i = 0; i < transform.childCount; i++)
			{
				targetGroup.RemoveMember(transform.GetChild(i));
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
			pipeColorIdx = 0;
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

				// if not available then delete grid
				if (b == null)
				{
					DeleteGrid();
					return;
				}
				
				destroyList.Add(b.gameObject);

				SetObstacle(b.x, b.z, (Rotation)Random.Range(0, 2));
				CalculateBallPath();
			}
		}

		private void SpawnPipes(int pipeCount)
		{
			for (int i = 0; i < pipeCount; i++)
			{
				Block b1 = GetPathBlock();

				// if not available then delete grid
				if (b1 == null)
				{
					DeleteGrid();
					return;
				}
				
				destroyList.Add(b1.gameObject);

				Pipe p1 = SetPipe(b1.x, b1.z, GetOppositeDirection(b1.incomingBallDirection), pipeColors[pipeColorIdx]);
				destroyList.Add(p1.gameObject);
				
				Direction selectedDirForP2 = (Direction)Random.Range(0, 4);
				Block b2 = GetEmptyBlock(selectedDirForP2);
				destroyList.Add(b2.gameObject);

				// if not available then delete grid
				if (b2 == null)
				{
					DeleteGrid();
					return;
				}

				Pipe p2 = SetPipe(b2.x, b2.z, selectedDirForP2, pipeColors[pipeColorIdx]);
				destroyList.Add(p2.gameObject);

				ConnectPipes(p1, p2);
				pipeColorIdx++;

				CalculateBallPath();
			}
		}

		private void SpawnDecorations()
		{
			for (int i = 0; i < gridWidth * gridHeight * 8f / 100f; i++)
			{
				Block blockToReplace = GetEmptyBlock();
				GameObject decor = Instantiate(decorations[Random.Range(0, decorations.Count)], transform);
				decor.transform.position = blockToReplace.transform.position;
				GameObject model = decor.transform.GetChild(0).GetChild(0).gameObject;
				model.transform.rotation = Quaternion.Euler(-90, Random.Range(0, 4) * 90, model.transform.rotation.z);

				targetGroup.RemoveMember(blockToReplace.transform);
				targetGroup.AddMember(decor.transform, 1.0f, 1.0f);
				destroyList.Remove(blockToReplace.gameObject);
				destroyList.Add(decor.gameObject);
				
				Destroy(blockToReplace.gameObject);
			}
		}

		private Block GetEmptyBlock()
		{
			int x;
			int z;

			do
			{
				x = Random.Range(1, gridWidth - 2);
				z = Random.Range(1, gridHeight - 2);
			} while (path.Contains(grid[x,z]) || 
			         grid[x, z].TryGetComponent(out Obstacle o) || grid[x, z].TryGetComponent(out Pipe p) ||
			         (grid[x, z].TryGetComponent(out Block block) && path.Contains(block)));

			return grid[x, z];
		}

		private Block GetEmptyBlock(Direction dir)
		{
			int x;
			int z;

			do
			{
				x = Random.Range(1, gridWidth - 2);
				z = Random.Range(1, gridHeight - 2);
			} while (path.Contains(grid[x,z]) || 
			         grid[x, z].TryGetComponent(out Obstacle o) || grid[x, z].TryGetComponent(out Pipe p) ||
			         (grid[x, z].TryGetComponent(out Block block) && path.Contains(block)) ||
			         IsContainsPipeOnDirection(dir, x, z));

			return grid[x, z];
		}

		private bool IsContainsPipeOnDirection(Direction dir, int x, int z)
		{
			int startX = 0, startZ = 0;

			if (dir == Direction.UP || dir == Direction.DOWN)
			{
				startX = x;
				startZ = 0;
				
				while (startZ < gridHeight - 1)
				{
					if (grid[startX,++startZ].TryGetComponent(out Pipe p))
						return true;
				}
			}
			else if (dir == Direction.LEFT || dir == Direction.RIGHT)
			{
				startX = 0;
				startZ = z;
				
				while (startX < gridWidth - 1)
				{
					if (grid[++startX,startZ].TryGetComponent(out Pipe p))
						return true;
				}
			}

			return false;
		}

		private Block GetPathBlock()
		{
			int randIdx;
			int tries = 0;
			do
			{
				if (++tries > 10 || lastIndex + 1 >= path.Count)
				{
					return null;
				}
				randIdx = Random.Range(lastIndex + 1, path.Count);
			} while (path[randIdx].TryGetComponent(out Obstacle o) || 
			         path[randIdx].TryGetComponent(out Pipe p) || 
			         path[randIdx].x <= 0 || path[randIdx].z <= 0 || 
			         path[randIdx].x >= gridWidth - 1 || path[randIdx].z >= gridHeight - 1);

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

			HashSet<(int, int, Direction)> visited = new HashSet<(int, int, Direction)>();
			List<(int, int, Direction)> recentPath = new List<(int, int, Direction)>();
			(int x, int z, Direction direction)? previousPosition = null;

			do
			{
				var positionState = (tempBall.x, tempBall.z, tempBall.direction);

				// Detect cycles by checking if we're revisiting a position with the same direction
				if (visited.Contains(positionState))
				{
					Debug.LogError("Detected infinite loop in CalculateBallPath.");
					DeleteGrid();
					return;
				}
				visited.Add(positionState);
				recentPath.Add(positionState);

				// Trim recentPath to avoid excessive memory usage; check last 10 positions
				if (recentPath.Count > 10)
				{
					recentPath.RemoveAt(0);
				}

				// Detect repeated patterns in recent path, indicating a cycle
				for (int i = 0; i < recentPath.Count - 1; i++)
				{
					for (int j = i + 1; j < recentPath.Count; j++)
					{
						if (recentPath[i].Equals(recentPath[j]))
						{
							Debug.LogError("Detected a repeated state cycle in CalculateBallPath, indicating infinite loop.");
							DeleteGrid();
							return;
						}
					}
				}

				// Check for simple back-and-forth cycles
				if (previousPosition.HasValue && previousPosition.Value.x == tempBall.x &&
				    previousPosition.Value.z == tempBall.z &&
				    previousPosition.Value.direction == GetOppositeDirection(tempBall.direction))
				{
					Debug.LogError("Detected back-and-forth movement, possible infinite loop.");
					DeleteGrid();
					return;
				}

				grid[tempBall.x, tempBall.z].incomingBallDirection = tempBall.direction;
				path.Add(grid[tempBall.x, tempBall.z]);

				// Store the current position as the previous position before moving
				previousPosition = (tempBall.x, tempBall.z, tempBall.direction);

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

				// Break if out-of-bounds (OOB)
				if (tempBall.x < 0 || tempBall.z < 0 || tempBall.x >= gridWidth || tempBall.z >= gridHeight)
					break;

				// Update direction based on obstacles
				if (grid[tempBall.x, tempBall.z].TryGetComponent(out Obstacle o))
				{
					tempBall.direction = o.GetDirection(tempBall.direction);
					continue;
				}

				// Avoid entering the same pipe in the same direction repeatedly
				if (grid[tempBall.x, tempBall.z].TryGetComponent(out Pipe p))
				{
					if (p.enterDirection == tempBall.direction)
					{
						// Check if we are about to enter a loop through the pipe
						if (visited.Contains((tempBall.x, tempBall.z, tempBall.direction)))
						{
							Debug.LogError("Infinite loop detected due to repetitive pipe entry.");
							DeleteGrid();
							return;
						}

						p.BallEnter(tempBall);
						continue;
					}
				}

				// Terminate if too many tries, indicating a possible infinite loop
				if (++tries >= 100)
				{
					Debug.LogError("Failed to find path or possible infinite loop detected after maximum tries.");
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
			
			targetGroup.RemoveMember(block.transform);
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
			
			targetGroup.RemoveMember(block.transform);
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
			Block block;

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
			targetGroup.RemoveMember(block.transform);
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