using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGolf
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private Image fillImage;
		[SerializeField] private TMP_Text correctText;
		[SerializeField] private TMP_Text wrongText;
		[SerializeField] private TMP_Text levelText;
		[SerializeField] private TMP_Text roundText;
		[SerializeField] private TMP_Text scoreText;

		public void SetFillImage(float timer, float maxTime)
		{
			fillImage.fillAmount = timer / maxTime;
		}

		public void SetWrongText(int wrong)
		{
			wrongText.text = wrong.ToString();
		}

		public void SetCorrectText(int correct)
		{
			correctText.text = correct.ToString();
		}

		public void SetLevelText(int level)
		{
			levelText.text = "Level " + level.ToString();
		}

		public void SetRoundText(int round, int totalRound)
		{
			roundText.text = round + "/" + totalRound;
		}

		public void SetScoreText(int score)
		{
			scoreText.text = "Score: " + score.ToString();
		}
	}
}