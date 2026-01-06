using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

namespace UnityExtensionsEditor
{
    public static class FPSCounterEditor
    {
        [MenuItem("GameObject/UI/FPS Counter", false, 1)]
        private static void CreateFPSObject()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();

            if (!canvas)
            {
                GameObject canvasGameObject = new GameObject("Canvas");
                Undo.RegisterCreatedObjectUndo(canvasGameObject, "Canvas: FPS Counter");

                canvas = canvasGameObject.AddComponent<Canvas>();
                CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
                canvasGameObject.AddComponent<GraphicRaycaster>();

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1536.0f, 2048.0f);
                canvasScaler.matchWidthOrHeight = 0.0f;
            }

            FPSCounter fpsCounter = Resources.Load<FPSCounter>("FPSPanel");
            Object fpsPanel = Object.Instantiate(fpsCounter, canvas.transform);
            fpsPanel.name = "FPSCounter";
            Undo.RegisterCreatedObjectUndo(fpsPanel, "FPS Counter");
        }
    }
}