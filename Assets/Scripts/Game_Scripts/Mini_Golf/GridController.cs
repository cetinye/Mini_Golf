using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace MiniGolf
{
	public class GridController : MonoBehaviour
	{
		[Header("Grid Settings")]
		public int gridWidth;
		public int gridHeight;
		public Block[,] grid;
		private int obstacleCount;
		private int rateOfFakeObstacles;
		private int pipeCount;
		private int numOfFakePipe;
		private float previewTime;
		private float numOfFakeObstacle;
		[SerializeField] private Transform gridTransform;
		[SerializeField] private float tileSize = 2.125f;
		private Ball spawnedBall;
		private int lastIndex;
		private List<GameObject> destroyList = new List<GameObject>();
		private int pipeColorIdx = 0;
		private Block ballSpawnBlock;
		private Ball tempBall;
		public Block lastBlock;
		[SerializeField] private List<Color> pipeColors = new List<Color>();
		[SerializeField] private List<Block> path = new List<Block>();
		[SerializeField] private List<Block> decoratedBlocks = new List<Block>();

		[Header("Models")]
		[SerializeField] private Ball ball;
		[SerializeField] private Block hole;
		[SerializeField] private Obstacle obstacle;
		[SerializeField] private List<Block> decorations = new List<Block>();
		[SerializeField] private Block ballStart;
		[SerializeField] private Pipe pipe;
		[SerializeField] private GameObject flag;
		[SerializeField] private Block flat;

		[Header("Cinemachine")]
		[SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;
		[SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;


		[Header("Move Variables")]
		[SerializeField] private Transform right;
		[SerializeField] private Transform left;
		[SerializeField] private float moveTime;
		[SerializeField] private AnimationCurve moveAnimCurve;

		IEnumerator spawnPathObstacles;
		IEnumerator spawnObstaclesRoutine;
		IEnumerator spawnPipesRoutine;

		public void Restart()
		{
			StartCoroutine(DeleteGrid());
		}

		void OnEnable()
		{
			Hole.FlagPlaced += OnFlagPlaced;
		}

		void OnDisable()
		{
			Hole.FlagPlaced -= OnFlagPlaced;
		}

		public void AssignVariables()
		{
			gridWidth = LevelManager.LevelSO.gridSizeX;
			gridHeight = LevelManager.LevelSO.gridSizeY;
			obstacleCount = LevelManager.LevelSO.numOfObstacles;
			rateOfFakeObstacles = LevelManager.LevelSO.rateOfFakeObstacles;
			pipeCount = LevelManager.LevelSO.numOfPipe;
			numOfFakePipe = LevelManager.LevelSO.numOfFakePipe;
			previewTime = LevelManager.LevelSO.previewTime;
			numOfFakeObstacle = LevelManager.LevelSO.numOfFakeObstacles;
		}

		public void Create()
		{
			StartCoroutine(CreateGrid());
		}

		IEnumerator CreateGrid()
		{
			InitializeGrid();
			ChooseBallSpawnPoint();
			CalculateBallPath();

			if (spawnPathObstacles != null)
			{
				StopCoroutine(spawnPathObstacles);
				spawnPathObstacles = null;
			}

			spawnPathObstacles = SpawnPathObstacles();
			StartCoroutine(spawnPathObstacles);

			yield return null;
		}

		IEnumerator SpawnPathObstacles()
		{
			if (spawnObstaclesRoutine != null)
			{
				StopCoroutine(spawnObstaclesRoutine);
				spawnObstaclesRoutine = null;
			}

			if (spawnPipesRoutine != null)
			{
				StopCoroutine(spawnPipesRoutine);
				spawnPipesRoutine = null;
			}

			spawnObstaclesRoutine = SpawnObstacles(obstacleCount);
			spawnPipesRoutine = SpawnPipes(pipeCount);

			yield return spawnObstaclesRoutine;
		}

		IEnumerator DeleteGrid()
		{
			StopCoroutine(spawnPathObstacles);
			StopCoroutine(spawnObstaclesRoutine);
			StopCoroutine(spawnPipesRoutine);
			spawnPathObstacles = null;
			spawnObstaclesRoutine = null;
			spawnPipesRoutine = null;

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

			for (int i = 0; i < gridTransform.childCount; i++)
			{
				targetGroup.RemoveMember(gridTransform.GetChild(i));
				Destroy(gridTransform.GetChild(i).gameObject);
			}

			for (int i = 0; i < destroyList.Count; i++)
			{
				if (destroyList[i] == null) continue;

				Destroy(destroyList[i].gameObject);
			}

			for (int i = 0; i < decoratedBlocks.Count; i++)
			{
				if (decoratedBlocks[i] == null) continue;

				Destroy(decoratedBlocks[i].gameObject);
			}

			destroyList.Clear();
			Hole.isFlagSet = false;
			grid = null;
			path.Clear();
			decoratedBlocks.Clear();
			lastIndex = 0;
			pipeColorIdx = 0;
			Destroy(spawnedBall.gameObject);

			AssignVariables();

			yield return CreateGrid();
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
						newBlock = Instantiate(hole, gridTransform);
					}
					else
					{
						newBlock = Instantiate(flat, gridTransform);
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

		IEnumerator SpawnObstacles(int obstacleCount)
		{
			for (int i = 0; i < obstacleCount; i++)
			{
				Block b = GetPathBlock();

				// if not available then delete grid
				if (b == null)
				{
					yield return DeleteGrid();
				}

				destroyList.Add(b.gameObject);

				SetObstacle(b.x, b.z, (Rotation)Random.Range(0, 2));
				CalculateBallPath();
			}

			yield return spawnPipesRoutine;
		}

		IEnumerator SpawnPipes(int pipeCount)
		{
			for (int i = 0; i < pipeCount; i++)
			{
				Block b1 = GetPathBlock();

				// if not available then delete grid
				if (b1 == null)
				{
					yield return DeleteGrid();
				}

				if (destroyList == null)
					yield return null;

				destroyList.Add(b1.gameObject);

				Pipe p1 = SetPipe(b1.x, b1.z, GetOppositeDirection(b1.incomingBallDirection), pipeColors[pipeColorIdx]);
				destroyList.Add(p1.gameObject);

				Direction selectedDirForP2 = (Direction)Random.Range(0, 4);
				Block b2 = GetEmptyBlock(selectedDirForP2);

				// if not available then delete grid
				if (b2 == null)
				{
					yield return DeleteGrid();
				}

				destroyList.Add(b2.gameObject);

				Pipe p2 = SetPipe(b2.x, b2.z, selectedDirForP2, pipeColors[pipeColorIdx]);
				destroyList.Add(p2.gameObject);

				ConnectPipes(p1, p2);
				pipeColorIdx++;

				CalculateBallPath();
			}

			DisableCorners();
			SpawnFakeObstacles();
			SpawnFakePipes();
			SpawnDecorations();

			if (path[^1] != null)
			{
				lastBlock = path[^1];
				Debug.Log("Path ends at [" + lastBlock.x + "," + lastBlock.z + "]");
			}

			yield return MoveGridIn();
		}

		private void SpawnFakeObstacles()
		{
			for (int i = 0; i < numOfFakeObstacle; i++)
			{
				Block blockToReplace = GetEmptyBlock();

				Obstacle fakeObs = Instantiate(obstacle, gridTransform);
				fakeObs.SetRotation((Rotation)UnityEngine.Random.Range(0, 2));
				fakeObs.transform.position = blockToReplace.transform.position;
				fakeObs.x = blockToReplace.x;
				fakeObs.z = blockToReplace.z;
				grid[fakeObs.x, fakeObs.z] = fakeObs;

				targetGroup.RemoveMember(blockToReplace.transform);
				targetGroup.AddMember(fakeObs.transform, 1.0f, 1.0f);
				destroyList.Remove(blockToReplace.gameObject);
				destroyList.Add(fakeObs.gameObject);

				decoratedBlocks.Add(fakeObs);
				Destroy(blockToReplace.gameObject);
			}
		}

		private void SpawnFakePipes()
		{
			for (int i = 0; i < numOfFakePipe; i++)
			{
				Block b1 = GetEmptyBlock();

				decoratedBlocks.Add(b1);
				destroyList.Add(b1.gameObject);

				Pipe p1 = SetPipe(b1.x, b1.z, GetOppositeDirection((Direction)Random.Range(0, 4)), pipeColors[pipeColorIdx]);
				destroyList.Add(p1.gameObject);
				grid[b1.x, b1.z] = p1;

				Direction selectedDirForP2 = (Direction)Random.Range(0, 4);
				Block b2 = GetEmptyBlock(selectedDirForP2);

				destroyList.Add(b2.gameObject);

				Pipe p2 = SetPipe(b2.x, b2.z, selectedDirForP2, pipeColors[pipeColorIdx]);
				destroyList.Add(p2.gameObject);
				grid[b2.x, b2.z] = p2;

				ConnectPipes(p1, p2);
				pipeColorIdx++;
			}
		}

		private void SpawnDecorations()
		{
			for (int i = 0; i < gridWidth * gridHeight * rateOfFakeObstacles / 100f; i++)
			{
				Block blockToReplace = GetEmptyBlock();

				if (blockToReplace == null)
				{
					continue;
				}

				Block decor = Instantiate(decorations[Random.Range(0, decorations.Count)], gridTransform);
				decor.transform.position = blockToReplace.transform.position;
				GameObject model = decor.transform.GetChild(0).GetChild(0).gameObject;
				model.transform.rotation = Quaternion.Euler(-90, UnityEngine.Random.Range(0, 4) * 90, model.transform.rotation.z);
				decor.x = blockToReplace.x;
				decor.z = blockToReplace.z;
				grid[decor.x, decor.z] = decor;

				targetGroup.RemoveMember(blockToReplace.transform);
				targetGroup.AddMember(decor.transform, 1.0f, 1.0f);
				destroyList.Remove(blockToReplace.gameObject);
				destroyList.Add(decor.gameObject);

				decoratedBlocks.Add(decor);
				Destroy(blockToReplace.gameObject);
			}
		}

		private Block GetEmptyBlock()
		{
			int tries = 0;
			int x;
			int z;

			do
			{
				x = Random.Range(1, gridWidth - 1);
				z = Random.Range(1, gridHeight - 1);

				if (++tries > 100)
				{
					return null;
				}

			} while (path.Contains(grid[x, z]) ||
					 grid[x, z].TryGetComponent(out Obstacle o) || grid[x, z].TryGetComponent(out Pipe p) ||
					 decoratedBlocks.Contains(grid[x, z]) ||
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
			} while (path.Contains(grid[x, z]) ||
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
					if (grid[startX, ++startZ].TryGetComponent(out Pipe p))
						return true;
				}
			}
			else if (dir == Direction.LEFT || dir == Direction.RIGHT)
			{
				startX = 0;
				startZ = z;

				while (startX < gridWidth - 1)
				{
					if (grid[++startX, startZ].TryGetComponent(out Pipe p))
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

		private void SetObstacleVisibility(bool visible, bool isInstant = false)
		{
			for (int x = 0; x < gridWidth; x++)
			{
				for (int z = 0; z < gridHeight; z++)
				{
					if (grid[x, z] != null && grid[x, z].TryGetComponent<Block>(out Block b))
					{
						b.SetMeshRendererState(visible, isInstant);
					}

					else
					{
						Debug.LogWarning("No Block Found at " + x + " " + z);
					}
				}
			}
		}

		private void CalculateBallPath()
		{
			int tries = 0;
			path.Clear();

			if (tempBall == null)
			{
				tempBall = Instantiate(ball, gridTransform);
				tempBall.SetMeshRendererState(false, true);
			}

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
					StopAllCoroutines();
					StartCoroutine(DeleteGrid());
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
							StopAllCoroutines();
							StartCoroutine(DeleteGrid());
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
					StopAllCoroutines();
					StartCoroutine(DeleteGrid());
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
							StopAllCoroutines();
							StartCoroutine(DeleteGrid());
							return;
						}

						p.BallEnter(tempBall);
						continue;
					}

					// Check for not enterable pipe spawned on path 
					if (p.enterDirection != tempBall.direction)
					{
						Debug.LogError("Not enterable pipe spawned on path.");
						StopAllCoroutines();
						StartCoroutine(DeleteGrid());
						return;
					}
				}

				// Terminate if too many tries, indicating a possible infinite loop
				if (++tries >= 100)
				{
					Debug.LogError("Failed to find path or possible infinite loop detected after maximum tries.");
					StopAllCoroutines();
					StartCoroutine(DeleteGrid());
					return;
				}

			} while (tempBall.x >= 0 && tempBall.z >= 0 && tempBall.x < gridWidth && tempBall.z < gridHeight);

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
			Obstacle o = Instantiate(obstacle, gridTransform);
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
			Pipe p = Instantiate(pipe, gridTransform);
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
			Block block = null;

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

			ballSpawnBlock = block;
		}

		private void SpawnBall(int x, int z, Block block)
		{
			spawnedBall = Instantiate(ball, gridTransform);
			spawnedBall.SetMeshRendererState(false, true);
			spawnedBall.transform.position = new Vector3(grid[x, z].transform.position.x, spawnedBall.transform.position.y, grid[x, z].transform.position.z);

			spawnedBall.x = x;
			spawnedBall.z = z;

			spawnedBall.SetGridController(this);
		}

		private void SwitchBallStartBlock()
		{
			Vector3 pos = ballSpawnBlock.transform.position;
			targetGroup.RemoveMember(ballSpawnBlock.transform);

			grid[ballSpawnBlock.x, ballSpawnBlock.z] = Instantiate(ballStart, gridTransform);
			grid[ballSpawnBlock.x, ballSpawnBlock.z].transform.position = new Vector3(pos.x, 0f, pos.z);

			Destroy(ballSpawnBlock.gameObject);
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

		private void OnFlagPlaced(int x, int z)
		{
			SetObstacleVisibility(true);
		}

		IEnumerator MoveGridIn()
		{
			LevelManager.Instance.GameState = GameState.MoveIn;

			yield return new WaitForEndOfFrame();
			SetObstacleVisibility(false, true);
			virtualCamera.enabled = true;
			yield return new WaitForEndOfFrame();
			virtualCamera.enabled = false;

			gridTransform.position = right.transform.position;

			Tween t = gridTransform.DOMove(Vector3.zero, moveTime).SetEase(moveAnimCurve);
			yield return t.WaitForCompletion();

			LevelManager.Instance.GameState = GameState.Preview;
			SetObstacleVisibility(true);
			yield return new WaitForSeconds(previewTime + 0.5f);

			SetObstacleVisibility(false);
			SwitchBallStartBlock();
			spawnedBall.SetMeshRendererState(true);

			LevelManager.Instance.GameState = GameState.Playing;
		}
	}
}