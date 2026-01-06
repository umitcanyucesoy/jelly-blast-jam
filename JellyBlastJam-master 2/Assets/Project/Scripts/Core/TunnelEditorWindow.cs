#if UNITY_EDITOR

using System.Collections.Generic;
using Project.Scripts.Managers.Core;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Project.Scripts.Core
{
    public class TunnelEditorWindow : EditorWindow
    {
        private List<TunnelSlot> tunnelSlots = new List<TunnelSlot>();
        private ShooterGrid.ShooterGridVisual parentGrid;
        private TunnelForward tunnelForward;
        private ReorderableList reorderableList;

        private void StartPaint()
        {
            if (parentGrid != null)
            {
                tunnelSlots = new List<TunnelSlot>(parentGrid.tunnelSlots);
                if (tunnelSlots == null || tunnelSlots.Count == 0)
                {
                    tunnelSlots = new List<TunnelSlot>
                    {
                        new() { color = BallColor.Red, count = 50 },
                        new() { color = BallColor.Green, count = 50 },
                        new() { color = BallColor.Blue, count = 50 }
                    };
                }
            }
            else
            {
                tunnelSlots = new List<TunnelSlot>
                {
                    new() { color = BallColor.Red, count = 50 },
                    new() { color = BallColor.Green, count = 50 },
                    new() { color = BallColor.Blue, count = 50 }
                };
            }

            reorderableList = new ReorderableList(tunnelSlots, typeof(TunnelSlot), true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Tunnel Slot List"); },
                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        var slot = tunnelSlots[index];

                        var slotColor = GM.Instance.GetShooterColor(slot.color).colorMaterial.color;

                        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), slotColor);

                        var labelStyle = new GUIStyle(GUI.skin.label)
                        {
                            normal = { textColor = Color.black },
                            alignment = TextAnchor.MiddleLeft,
                            fontSize = 12
                        };

                        EditorGUI.BeginChangeCheck();
                        slot.color = (BallColor)EditorGUI.EnumPopup(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "", slot.color, labelStyle);
                        slot.count = EditorGUI.IntField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "", slot.count);
                        if (EditorGUI.EndChangeCheck())
                        {
                            tunnelSlots[index] = slot;
                        }
                    }
                };

            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                tunnelSlots.Add(new TunnelSlot { color = BallColor.Red, count = 50 });
                reorderableList.list = tunnelSlots;
                Repaint();
            };

            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (tunnelSlots.Count > 0)
                {
                    tunnelSlots.RemoveAt(list.index);
                    reorderableList.list = tunnelSlots;
                    Repaint();
                }
            };

            Repaint();
        }

        public static void ShowWindow(ShooterGrid.ShooterGridVisual grid)
        {
            var window = GetWindow<TunnelEditorWindow>("Tunnel Editor");
            window.SetParentGrid(grid);
            window.Show();
        }

        public void SetParentGrid(ShooterGrid.ShooterGridVisual grid)
        {
            parentGrid = grid;
            tunnelSlots = new List<TunnelSlot>(parentGrid.tunnelSlots);
            tunnelForward = parentGrid.forwardDirection;
            StartPaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tunnel Slot Editor", EditorStyles.boldLabel);

            if (tunnelSlots.Count == 0)
            {
                EditorGUILayout.LabelField("Slots are empty! Regenerating the list");
                tunnelSlots = new List<TunnelSlot>
                {
                    new() { color = BallColor.Red, count = 50 },
                    new() { color = BallColor.Green, count = 50 },
                    new() { color = BallColor.Blue, count = 50 }
                };
                reorderableList.list = tunnelSlots;
                return;
            }

            reorderableList.DoLayoutList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tunnel Forward Direction", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal("box");

            GUILayout.Label("Direction:", GUILayout.Width(70));
            
            if (GUILayout.Toggle(tunnelForward == TunnelForward.Left, "← Left", "Button"))
            {
                tunnelForward = TunnelForward.Left;
            }
            if (GUILayout.Toggle(tunnelForward == TunnelForward.Front, "↑ Front", "Button"))
            {
                tunnelForward = TunnelForward.Front;
            }
            if (GUILayout.Toggle(tunnelForward == TunnelForward.Right, "Right →", "Button"))
            {
                tunnelForward = TunnelForward.Right;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();


            if (GUILayout.Button("Apply"))
            {
                if (parentGrid != null)
                {
                    parentGrid.tunnelSlots = new List<TunnelSlot>(tunnelSlots);
                    parentGrid.forwardDirection = tunnelForward;
                }

                Close();
            }

            if (GUILayout.Button("Close"))
            {
                if (parentGrid != null)
                {
                    parentGrid.tunnelSlots = new List<TunnelSlot>(tunnelSlots);
                    parentGrid.forwardDirection = tunnelForward;
                }
                Close();
            }
        }
    }
}
#endif