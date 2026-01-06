using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class ScreenshotHelper : MonoBehaviour
{

#if UNITY_EDITOR
    [MenuItem("Arcade Clan/TakeLevelScreenshots")]
    public static async void TakeLevelScreenshots()
    {
        int count = FindObjectOfType<GM>().levels.Count;
        PlayerPrefs.DeleteAll();
        Application.LoadLevel(1);
        for (int a = 0; a < count; a++)
        {
            ScreenCapture.CaptureScreenshot("Level" + (GM.level + 1)+"-"+GM.Instance.currentLevel.gameObject.name + ".png");
            await Task.Delay(10);
            GM.level += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            await Task.Delay(350);
        }
    }
#endif
}