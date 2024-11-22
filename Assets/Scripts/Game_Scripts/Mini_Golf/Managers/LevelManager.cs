using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolf
{
	public class LevelManager : MonoBehaviour
	{
		public static LevelManager Instance;

		[SerializeField] private UIManager uiManager;

		public int levelId;
		[SerializeField] private List<LevelSO> levels = new List<LevelSO>();
		public static LevelSO LevelSO;

		[Space()]
		[SerializeField] private GridController gridController;

		[Space()]
		[SerializeField] private int correctCount, wrongCount;
		[SerializeField] private int totalCorrectCount, totalWrongCount;
		[SerializeField] private int roundsPlayed = 1;
		[SerializeField] private float answerTimer;

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

		void Start()
		{
			StartGame();
		}

		void Update()
		{
			if (state == GameState.Playing)
			{
				answerTimer -= Time.deltaTime;

				if (answerTimer < 0)
				{
					state = GameState.Timeout;
					answerTimer = 0;
					RoundComplete(false);
				}

				uiManager.SetFillImage(answerTimer, LevelSO.answerTime);
			}
		}

		public void Restart()
		{
			gridController.Restart();
		}

		private void AssignLevel()
		{
			levelId = PlayerPrefs.GetInt("MiniGolf_Level", 1);
			levelId = Mathf.Clamp(levelId, 1, levels.Count);
			LevelSO = levels[levelId - 1];

			answerTimer = LevelSO.answerTime;
			uiManager.SetFillImage(answerTimer, LevelSO.answerTime);

			uiManager.SetLevelText(levelId);
			uiManager.SetRoundText(roundsPlayed, LevelSO.totalNumOfQuestions);
			uiManager.SetWrongText(totalWrongCount);
			uiManager.SetCorrectText(totalCorrectCount);
		}

		public void StartGame()
		{
			AudioManager.Instance.Play(SoundType.Background);

			gridController.AssignVariables();
			gridController.Create();
		}

		public void RoundComplete(bool isSuccess)
		{
			if (isSuccess)
			{
				correctCount++;
				totalCorrectCount++;

				uiManager.SetCorrectText(totalCorrectCount);
				AudioManager.Instance.PlayOneShot(SoundType.Success);
			}
			else
			{
				wrongCount++;
				totalWrongCount++;

				uiManager.SetWrongText(totalWrongCount);
				AudioManager.Instance.PlayOneShot(SoundType.Fail);
			}

			DecideLevel();
		}

		public void DecideLevel()
		{
			if (correctCount >= LevelSO.levelUpCriteria)
			{
				levelId++;

				correctCount = 0;
				wrongCount = 0;
			}
			else if (wrongCount >= LevelSO.levelDownCriteria)
			{
				levelId--;

				correctCount = 0;
				wrongCount = 0;
			}

			PlayerPrefs.SetInt("MiniGolf_Level", levelId);

			uiManager.SetScoreText(GetScore());

			AssignLevel();
			gridController.MoveOutGrid();
		}

		public void DecideRounds()
		{
			if (++roundsPlayed > LevelSO.totalNumOfQuestions)
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			else
			{
				uiManager.SetRoundText(roundsPlayed, LevelSO.totalNumOfQuestions);
				Restart();
			}
		}

		public int GetScore()
		{
			float inGameScore = (totalCorrectCount * LevelSO.pointsPerCorrect) - (totalWrongCount * LevelSO.penaltyPoints);
			float maxInGame = LevelSO.totalNumOfQuestions * LevelSO.pointsPerCorrect;
			float witminaScore = inGameScore / maxInGame * 1000f;

			return Mathf.Clamp(Mathf.CeilToInt(witminaScore), 0, 1000);
		}

		public Block GetFinishBlock()
		{
			return gridController.lastBlock;
		}
	}

	[Serializable]
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