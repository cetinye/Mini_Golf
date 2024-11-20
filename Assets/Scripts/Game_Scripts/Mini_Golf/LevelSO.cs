using UnityEngine;

namespace MiniGolf
{
	public class LevelSO : ScriptableObject
	{
		public int levelId;
		public int gridSizeX;
		public int gridSizeY;
		public int numOfObstacles;
		public int rateOfFakeObstacles;
		public int numOfFakeObstacles;
		public int numOfPipe;
		public int numOfFakePipe;
		public float previewTime;
		public float answerTime;
		public int levelUpCriteria;
		public int levelDownCriteria;
		public int totalNumOfQuestions;
		public float pointsPerCorrect;
		public float maxInGame;
		public float penaltyPoints;
	}
}