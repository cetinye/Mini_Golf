using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
	public class GridController : MonoBehaviour
	{
		[Header("Grid Settings")]
		[SerializeField] private int gridWidth = 5;
		[SerializeField] private int gridHeight = 5;
		[SerializeField] private float tileSize = 1.0f;

		[Header("Models")]
		[SerializeField] private GameObject ball;
		[SerializeField] private Block hole;
		[SerializeField] private List<GameObject> sand = new List<GameObject>();
		[SerializeField] private GameObject obstacle;
		[SerializeField] private List<GameObject> tree = new List<GameObject>();
		[SerializeField] private GameObject ballStart;
		[SerializeField] private GameObject pipe;
		[SerializeField] private GameObject flag;
		[SerializeField] private Block flat;

		[Header("Cinemachine")]
		[SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

		private Vector2 ballPosition;
		private Vector2 holePosition;

		public Block[,] grid;

		void Start()
		{
			CreateGrid();
		}

		public void CreateGrid()
		{
			InitializeGrid();
			DisableCorners();
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

		private void DisableCorners()
		{
			grid[0, 0].gameObject.SetActive(false);
			grid[0, gridHeight - 1].gameObject.SetActive(false);
			grid[gridWidth - 1, 0].gameObject.SetActive(false);
			grid[gridWidth - 1, gridHeight - 1].gameObject.SetActive(false);
		}
	}
}
