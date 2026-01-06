using System;
using System.Collections.Generic;
using Project.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Core
{
    [CreateAssetMenu(fileName = "ShooterGrid", menuName = "Grid/ShooterGrid", order = 1)]
    [InlineEditor]
    public class ShooterGrid : SerializedScriptableObject
    {
        public int column = 4;
        public int row = 6;
        public float beginOffsetToMidPoint = 3f;
        public float gridOffsetZ = 1.2f;
        public float gridOffsetX = 1.5f;

        [Serializable]
        public class ShooterGridVisual
        {
            public bool isTunnel;
            public BallColor color = BallColor.Red;
            public int count = 50;
            public bool isHidden;
            public List<TunnelSlot> tunnelSlots;
            public TunnelForward forwardDirection = TunnelForward.Front;
        }

        [Button]
        public void InitBlocks()
        {
            cellPlacement = null;
            cellPlacement = new ShooterGridVisual[column, row];

            for (var i = 0; i < column; i++)
            {
                for (var j = 0; j < row; j++)
                {
                    var visual = new ShooterGridVisual
                    {
                        color = BallColor.Red,
                    };
                    cellPlacement[i, j] = visual;
                }
            }
        }


        [ShowInInspector, TableMatrix(RowHeight = 75, ResizableColumns = true, DrawElementMethod = "DrawCell")]
        public ShooterGridVisual[,] cellPlacement = new ShooterGridVisual[,] { };


#if UNITY_EDITOR
      private static ShooterGridVisual DrawCell(Rect rect, ShooterGridVisual value)
{
    EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), Color.white);
    EditorGUI.DrawRect(rect.Padding(1), GetColor(value));

    var spacing = 2f;

    var verticalButtonHeight = (rect.height - 3 * spacing) / 2f;
    var verticalButtonWidth = rect.width * 0.2f;

    var hiddenButtonRect =
        new Rect(rect.x + spacing, rect.y + spacing, verticalButtonWidth, verticalButtonHeight);
    Rect tunnelButtonRect;
    if (value.isTunnel)
    {
        // T butonu tüm hücreyi kapsasın
        tunnelButtonRect = new Rect(rect.x + spacing, rect.y + spacing, verticalButtonWidth, rect.height - 2 * spacing);
    }
    else
    {
        // Normal durumda altlı üstlü duracak şekilde
        tunnelButtonRect = new Rect(rect.x + spacing, hiddenButtonRect.yMax + spacing, verticalButtonWidth, verticalButtonHeight);
    }

    var countHeight = rect.height * 0.4f;
    var countWidth = rect.width * 0.3f;
    var countX = rect.center.x - countWidth / 2f;
    var countY = rect.center.y - countHeight / 2f;
    var countRect = new Rect(countX, countY, countWidth, countHeight);

    var pmButtonWidth = rect.width * 0.1f;
    var pmButtonHeight = countHeight;
    var plusButtonRect = new Rect(countRect.xMax + spacing, countY, pmButtonWidth, pmButtonHeight);
    var minusButtonRect = new Rect(countRect.xMin - pmButtonWidth - spacing, countY, pmButtonWidth,
        pmButtonHeight);

    var colorButtonWidth = rect.width * 0.15f;
    var colorButtonHeight = rect.height - 2 * spacing;
    var colorSelectMenuButton = new Rect(rect.xMax - colorButtonWidth - spacing, rect.y + spacing, colorButtonWidth, colorButtonHeight);
    
    if (GUI.Button(tunnelButtonRect, "T"))
    {
        value.isTunnel = !value.isTunnel;
        if (value.isTunnel)
        {
            if (value.tunnelSlots == null || value.tunnelSlots.Count == 0)
            {
                value.tunnelSlots = new List<TunnelSlot>
                {
                    new TunnelSlot { count = 50, color = BallColor.Red },
                    new TunnelSlot { count = 50, color = BallColor.Green },
                    new TunnelSlot { count = 50, color = BallColor.Blue }
                };
            }
        }
    }
    
    if (value.isTunnel)
    {
        // Yeni beyaz arka plan rect'ini T butonunun hemen sağından başlatıp sağa doğru tüm hücreyi kapsayacak şekilde ayarlıyoruz
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(rect.width * 0.125f),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.black }
        };

        var labelX = tunnelButtonRect.xMax + spacing;
        var labelWidth = rect.xMax - labelX - spacing;
        var labelRect = new Rect(labelX, rect.y + spacing, labelWidth, rect.height - 2 * spacing);

        // Beyaz arka planı çiziyoruz
        EditorGUI.DrawRect(labelRect, Color.white);

        // TUNNEL yazısını ortada çiziyoruz
        EditorGUI.LabelField(labelRect, "TUNNEL", labelStyle);

        var directionLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontStyle = FontStyle.Bold,
            fontSize = Mathf.RoundToInt(rect.height * 0.1f),
            normal = { textColor = Color.black }
        };

        // Yön etiketini de ekliyoruz
        EditorGUI.LabelField(labelRect, $"{value.forwardDirection}", directionLabelStyle);

        if (labelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
        {
            OpenTunnelEditorWindow(value);
        }

        return value;
    }

    var style = new GUIStyle(GUI.skin.textField)
    {
        alignment = TextAnchor.MiddleCenter,
        fontSize = Mathf.RoundToInt(countHeight * 0.5f),
        padding = new RectOffset(0, 0, 4, 4)
    };

    value.count = EditorGUI.IntField(countRect, GUIContent.none, value.count, style);

    if (GUI.Button(plusButtonRect, "+"))
    {
        value.count += 5;
        GUI.changed = true;
        EditorGUI.FocusTextInControl("");
    }

    if (GUI.Button(minusButtonRect, "-"))
    {
        value.count -= 5;
        GUI.changed = true;
        EditorGUI.FocusTextInControl("");
    }

    if (GUI.Button(hiddenButtonRect, "H"))
    {
        value.isHidden = !value.isHidden;
        GUI.changed = true;
        EditorGUI.FocusTextInControl("");
    }

    if (GUI.Button(colorSelectMenuButton, "C"))
    {
        var menu = new GenericMenu();
        foreach (BallColor color in Enum.GetValues(typeof(BallColor)))
        {
            menu.AddItem(new GUIContent(color.ToString()), false, () =>
            {
                value.color = color;
                GUI.changed = true;
            });
        }

        menu.ShowAsContext();
        Event.current.Use();
    }

    if (value.isHidden)
    {
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.LowerCenter,
            fontStyle = FontStyle.Bold,
            fontSize = Mathf.RoundToInt(rect.height * 0.2f),
            normal = { textColor = Color.white }
        };

        EditorGUI.LabelField(rect, "Hidden", labelStyle);
    }

    if (rect.Contains(Event.current.mousePosition) && !countRect.Contains(Event.current.mousePosition) &&
        !plusButtonRect.Contains(Event.current.mousePosition) &&
        !minusButtonRect.Contains(Event.current.mousePosition) &&
        !colorSelectMenuButton.Contains(Event.current.mousePosition) &&
        !hiddenButtonRect.Contains(Event.current.mousePosition) &&
        !tunnelButtonRect.Contains(Event.current.mousePosition))
    {
        if (Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 1)
            {
                value.color = (BallColor)(((int)value.color + 1) % Enum.GetValues(typeof(BallColor)).Length);
                GUI.changed = true;
                Event.current.Use();
            }
            else if (Event.current.button == 0)
            {
                value.color = (BallColor)(((int)value.color - 1 + Enum.GetValues(typeof(BallColor)).Length) %
                                          Enum.GetValues(typeof(BallColor)).Length);
                GUI.changed = true;
                Event.current.Use();
            }
        }
    }

    return value;
}




        private static void OpenTunnelEditorWindow(ShooterGridVisual value)
        {
            TunnelEditorWindow.ShowWindow(value);
        }


        private static Color GetColor(ShooterGridVisual gridVisual)
        {
            var color = gridVisual.color;
            return gridVisual.color switch
            {
                _ => color switch
                {
                    BallColor.Red => Color.red,
                    BallColor.Blue => Color.blue,
                    BallColor.Green => Color.green,
                    BallColor.Pink => Color.magenta,
                    BallColor.Purple => new Color(0.34f, 0f, 1f),
                    BallColor.Orange => new Color(1f, 0.62f, 0.04f),
                    BallColor.Yellow => Color.yellow,
                    BallColor.Brown => new Color(0.53f, 0.24f, 0.17f),
                    BallColor.Turquoise => new Color(0.06f, 0.71f, 0.62f),
                    _ => Color.white
                }
            };
        }
#endif
    }
}