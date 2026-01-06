using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityExtensionsEditor
{
    public static class EditorHelpers
    {
        [MenuItem("Arcade Clan/Helpers/Create World Canvas")]
        public static void CreateWorldCanvas()
        {
            Canvas canvas = new GameObject("WorldCanvas").AddComponent<Canvas>();
            Undo.RecordObject(canvas, "World Canvas Created");
            
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasTransform = (RectTransform)canvas.transform;
            canvasTransform.anchoredPosition = Vector2.zero;
            canvasTransform.anchoredPosition3D = Vector2.zero;

            canvasTransform.localScale = Vector3.one * 0.01f;
            canvasTransform.sizeDelta = Vector2.one;
            
            canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            Selection.activeGameObject = canvas.gameObject;
        }
    }
}