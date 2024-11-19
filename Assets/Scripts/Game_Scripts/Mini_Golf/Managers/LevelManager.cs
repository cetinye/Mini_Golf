using System.Collections.Generic;
using UnityEngine;

namespace MiniGolf
{
	public class LevelManager : MonoBehaviour
	{
		public int levelId;
		[SerializeField] private List<LevelSO> levels = new List<LevelSO>();
		public static LevelSO LevelSO;

		[Space()]
		[SerializeField] private GridController gridController;

		private void Awake()
		{
			AssignLevel();
		}

		private void Start()
		{
			StartGame();
		}

		public void Restart()
		{
			levelId++;
			AssignLevel();
			gridController.Restart();
		}

		private void AssignLevel()
		{
			// levelId = PlayerPrefs.GetInt("MiniGolf_Level", 1);
			levelId = Mathf.Clamp(levelId, 1, levels.Count);
			LevelSO = levels[levelId - 1];
		}

		private void StartGame()
		{
			gridController.AssignVariables();
			gridController.Create();
		}
	}
}