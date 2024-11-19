using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace MiniGolf
{
	public class LevelManager : MonoBehaviour
	{
		public static LevelManager Instance;

		public int levelId;
		[SerializeField] private List<LevelSO> levels = new List<LevelSO>();
		public static LevelSO LevelSO;

		[Space()]
		[SerializeField] private GridController gridController;

		[Space()]
		private GameState state = GameState.Idle;
		public GameState GameState
		{
			get { return state; }
			set
			{
				state = value;
				Debug.Log("Game State: " + state);
			}
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
			}
			else
			{
				Instance = this;
			}

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
			GameState = GameState.GridCreation;
			gridController.AssignVariables();
			gridController.Create();
		}

		public Block GetFinishBlock()
		{
			return gridController.lastBlock;
		}
	}

	public enum GameState
	{
		Idle,
		GridCreation,
		MoveIn,
		Preview,
		Playing,
		Selected,
		Success,
		Fail,
		Timeout
	}
}