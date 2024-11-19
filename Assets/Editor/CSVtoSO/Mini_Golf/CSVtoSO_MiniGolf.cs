using UnityEngine;
using UnityEditor;
using System.IO;

namespace MiniGolf
{
    public class CSVtoSO_MiniGolf
    {
        //Check .csv path
        private static string CSVPath = "/Editor/CSVtoSO/Mini_Golf/LevelCSV_MiniGolf.csv";

        [MenuItem("Tools/CSV_to_SO/Mini_Golf/Generate")]
        public static void GenerateSO()
        {
            int startingNamingIndex = 1;
            string[] allLines = File.ReadAllLines(Application.dataPath + CSVPath);

            for (int i = 1; i < allLines.Length; i++)
            {
                allLines[i] = RedefineString(allLines[i]);
            }

            for (int i = 1; i < allLines.Length; i++)
            {
                string[] splitData = allLines[i].Split(';');

                //Check data indexes
                LevelSO level = ScriptableObject.CreateInstance<LevelSO>();
                level.levelId = int.Parse(splitData[0]);
                level.gridSizeX = int.Parse(splitData[1]);
                level.gridSizeY = int.Parse(splitData[2]);
                level.numOfObstacles = int.Parse(splitData[3]);
                level.rateOfFakeObstacles = int.Parse(splitData[4]);
                level.numOfPipe = int.Parse(splitData[5]);
                level.numOfFakePipe = int.Parse(splitData[6]);
                level.previewTime = float.Parse(splitData[7]);
                level.answerTime = float.Parse(splitData[8]);
                level.levelUpCriteria = int.Parse(splitData[9]);
                level.levelDownCriteria = int.Parse(splitData[10]);
                level.totalNumOfQuestions = int.Parse(splitData[11]);
                level.pointsPerCorrect = float.Parse(splitData[12]);
                level.maxInGame = float.Parse(splitData[13]);
                level.penaltyPoints = float.Parse(splitData[14]);

                AssetDatabase.CreateAsset(level, $"Assets/Data/Mini_Golf/Levels/{"MiniGolf_Level " + startingNamingIndex}.asset");
                startingNamingIndex++;
            }

            AssetDatabase.SaveAssets();

            static string RedefineString(string val)
            {
                char[] charArr = val.ToCharArray();
                bool isSplittable = true;

                for (int i = 0; i < charArr.Length; i++)
                {
                    if (charArr[i] == '"')
                    {
                        charArr[i] = ' ';
                        isSplittable = !isSplittable;
                    }

                    if (isSplittable && charArr[i] == ',')
                        charArr[i] = ';';

                    if (isSplittable && charArr[i] == '.')
                        charArr[i] = ',';
                }

                return new string(charArr);
            }
        }
    }
}