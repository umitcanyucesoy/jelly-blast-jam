using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace UnityExtensions
{
	public class FPSCounter : MonoBehaviour
	{
		private static readonly Color RedColor = new Color(1.0f, 0.2f, 0.2f);
		private static readonly Color OrangeColor = new Color(1.0f, 0.63f, 0.18f);
		private static readonly Color GreenColor = new Color(0.23f, 1.0f, 0.2f);

		private float deltaTime = 0.0f;

		[CustomHeader("UI Elements", true)]
		[SerializeField] private TextMeshProUGUI fpsText = null;
		[SerializeField] private TextMeshProUGUI levelNameText = null;

		private void Update()
		{
			deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			float milliSecond = deltaTime * 1000.0f;
			float framePerSecond = 1.0f / deltaTime;

			if (framePerSecond < 25.0f)
			{
				fpsText.color = RedColor;
			}
			else if (framePerSecond < 35.0f)
			{
				fpsText.color = OrangeColor;
			}
			else
			{
				fpsText.color = GreenColor;
			}

			fpsText.text = $"MS: {milliSecond:0.0}\nFPS:  {framePerSecond:0.}";
		}

		public void UpdateLevelText(string levelName)
		{
			if (!levelNameText)
			{
				return;
			}

			levelNameText.text = $"{levelName}";
		}
	}
}