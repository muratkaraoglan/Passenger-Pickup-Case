using System;
using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Grid;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Train;
using UnityEditor;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Editor
{
    public enum LevelEditorModeType
    {
        GridMode,
        TrainMode,
        PassengerMode,
    }

    public class LevelEditor : EditorWindow
    {
        private LevelConfig _currentLevelConfig;
        private CellType _currentCellType = CellType.Empty;
        private TrainColor _currentTrainColor;
        private Direction _currentTrainHeadDirection;
        private Vector2 _scrollPos;
        private TrainHolder _currentTrainHolder;
        private List<Vector2Int> _possiblePlacements = new List<Vector2Int>();
        private Dictionary<Vector2Int, Rect> _buttonRects = new Dictionary<Vector2Int, Rect>();
        private GUIStyle _customButtonStyle;

        #region LevelEditorMode

        private LevelEditorModeType _levelEditorCurrentMode = 0;
        private readonly float _modeButtonWidth = 400f;
        private readonly float _modeButtonHeight = 30f;
        private readonly string[] _modeOptionsNames = { "Grid Mode", "Train Mode", "Passenger Mode" };

        #endregion

        #region PassengerButtonSettings

        private const float OffsetTop = 25f;
        private const float OffsetBottom = 55f;
        private const float OffsetSideLeft = 25f;
        private const float OffsetSideRight = 55f;
        private const float PassengerButtonSize = 20f;

        #endregion


        [MenuItem("Tools/Level Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<LevelEditor>();
            window.titleContent = new GUIContent("Level Editor");
            window.Show();
        }

        private readonly string _errorMsg = "Make sure at least 2 train placed.";

        private void OnGUI()
        {
            _customButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 25,
                normal =
                {
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
            };

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            _currentLevelConfig =
                (LevelConfig)EditorGUILayout.ObjectField(_currentLevelConfig, typeof(LevelConfig), false);

            if (!_currentLevelConfig)
            {
                EditorGUILayout.HelpBox("Please select a level config.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawModeButtons();

            EditorGUILayout.BeginHorizontal();

            if (_levelEditorCurrentMode == LevelEditorModeType.GridMode)
            {
                DrawCellMode();
            }
            else if (_levelEditorCurrentMode == LevelEditorModeType.TrainMode)
            {
                DrawTrainMode();
            }

            EditorGUILayout.EndHorizontal();

            DrawGrid();
            DrawConnectionLines();

            EditorGUILayout.EndScrollView();
        }

        #region Draws

        private void DrawModeButtons()
        {
            float xPos = (position.width - _modeButtonWidth) / 2f;

            float yPos = position.height - _modeButtonHeight - 10f;

            Rect selectionGridRect = new Rect(xPos, yPos, _modeButtonWidth, _modeButtonHeight);

            _levelEditorCurrentMode = (LevelEditorModeType)GUI.SelectionGrid(selectionGridRect,
                (int)_levelEditorCurrentMode, _modeOptionsNames, _modeOptionsNames.Length);
        }

        private void DrawCellMode()
        {
            _currentTrainHolder = null;
            _possiblePlacements.Clear();
            _currentCellType = (CellType)EditorGUILayout.EnumPopup(_currentCellType);
            DrawCellColor();
        }

        private void DrawTrainMode()
        {
            EditorGUILayout.BeginVertical();
            _currentTrainColor = (TrainColor)EditorGUILayout.EnumPopup("Color", _currentTrainColor);
            _currentTrainHeadDirection =
                (Direction)EditorGUILayout.EnumPopup("Head Direction", _currentTrainHeadDirection);

            if (GUILayout.Button("Confirm Train Placement"))
            {
                if (_currentTrainHolder.wagons.Count > 1)
                {
                    _currentLevelConfig.trains.Add(_currentTrainHolder);

                    foreach (var wagon in _currentTrainHolder.wagons)
                    {
                        _currentLevelConfig.GetCell(wagon.coord).isOccupied = true;
                    }
                    
                    _currentTrainHolder = null;
                    _levelEditorCurrentMode = LevelEditorModeType.GridMode;
                }
            }

            EditorGUILayout.HelpBox(_errorMsg, MessageType.Info, true);
            EditorGUILayout.EndVertical();
        }

        private void DrawCellColor()
        {
            if (_levelEditorCurrentMode == LevelEditorModeType.TrainMode) return;

            EditorGUILayout.BeginVertical();
            foreach (CellType cellType in Enum.GetValues(typeof(CellType)))
            {
                EditorGUILayout.LabelField(cellType.ToString() + "  ", new GUIStyle()
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState()
                    {
                        textColor = GetDefaultCellColor(cellType)
                    }
                });
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGrid()
        {
            _buttonRects.Clear();
            if (_currentLevelConfig.cells.Length == 0)
                _currentLevelConfig.cells = new Cell[_currentLevelConfig.width * _currentLevelConfig.height];

            GUILayout.Space(50);
            for (int y = _currentLevelConfig.height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                for (int x = 0; x < _currentLevelConfig.width; x++)
                {
                    Vector2Int coordinates = new Vector2Int(x, y);

                    var cellData = GetColorAndNameForCell(coordinates);

                    if (_possiblePlacements.Contains(coordinates))
                    {
                        cellData.Item1 = Color.green;
                    }

                    GUI.backgroundColor = cellData.Item1;

                    Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button, GUILayout.Width(50),
                        GUILayout.Height(50));
                    string buttonText = "";
                    if (IsCellOccupiedByTrainInLevel(coordinates))
                    {
                        var trainHolder = GetTrainHolder(coordinates);
                        var wagon = trainHolder.wagons.First();
                        if (wagon.coord == coordinates)
                        {
                            buttonText = DirectionText(trainHolder.headDirection);
                            switch (trainHolder.headDirection)
                            {
                                case Direction.Up:
                                    _customButtonStyle.alignment = TextAnchor.UpperCenter;
                                    break;
                                case Direction.Down:
                                    _customButtonStyle.alignment = TextAnchor.LowerCenter;
                                    break;
                                case Direction.Right:
                                    _customButtonStyle.alignment = TextAnchor.MiddleRight;
                                    break;
                                case Direction.Left:
                                    _customButtonStyle.alignment = TextAnchor.MiddleLeft;
                                    break;
                            }
                        }
                    }

                    if (GUI.Button(buttonRect, buttonText, _customButtonStyle))
                    {
                        OnCellClicked(coordinates);
                    }

                    if (_levelEditorCurrentMode == LevelEditorModeType.PassengerMode)
                    {
                        DrawPassengerButton(x, y, buttonRect, _currentLevelConfig.width, _currentLevelConfig.height);
                    }

                    _currentLevelConfig.GetCell(coordinates).coord = coordinates;
                    _buttonRects[coordinates] = buttonRect;
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
        }

        private void DrawPassengerButton(int x, int y, Rect buttonRect, int gridWidth, int gridHeight)
        {
            if (y == gridHeight - 1) // Bottom edge
            {
                var rect = new Rect(buttonRect.x, buttonRect.y - OffsetTop, buttonRect.width, PassengerButtonSize);
                TryDrawButton(rect, "T");
            }
            else if (y == 0) // Top edge
            {
                var rect = new Rect(buttonRect.x, buttonRect.y + OffsetBottom, buttonRect.width, PassengerButtonSize);
                TryDrawButton(rect, "B");
            }

            if (x == gridWidth - 1) // Right edge
            {
                var rect = new Rect(buttonRect.x + OffsetSideRight, buttonRect.y, PassengerButtonSize,
                    buttonRect.height);
                TryDrawButton(rect, "R");
            }
            else if (x == 0) // Left edge
            {
                var rect = new Rect(buttonRect.x - OffsetSideLeft, buttonRect.y, PassengerButtonSize,
                    buttonRect.height);
                TryDrawButton(rect, "L");
            }
        }

        private void TryDrawButton(Rect rect, string label)
        {
            GUI.backgroundColor = Color.white;
            if (GUI.Button(rect, label))
            {
                Debug.Log($"Button {label} clicked at {rect}");
            }
        }

        private void DrawConnectionLines()
        {
            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                Handles.color = Color.white;

                foreach (var trainHolder in _currentLevelConfig.trains)
                {
                    for (int i = 0; i < trainHolder.wagons.Count - 1; i++)
                    {
                        Vector2Int fromCoord = trainHolder.wagons[i].coord;
                        Vector2Int toCoord = trainHolder.wagons[i + 1].coord;

                        Vector2 fromCenter = new Vector2(
                            _buttonRects[fromCoord].x + _buttonRects[fromCoord].width / 2,
                            _buttonRects[fromCoord].y + _buttonRects[fromCoord].height / 2
                        );

                        Vector2 toCenter = new Vector2(
                            _buttonRects[toCoord].x + _buttonRects[toCoord].width / 2,
                            _buttonRects[toCoord].y + _buttonRects[toCoord].height / 2
                        );

                        //Handles.DrawLine(fromCenter, toCenter);
                        Handles.DrawLine(fromCenter + Vector2.up, toCenter + Vector2.up);
                    }
                }

                Handles.EndGUI();
            }
        }

        #endregion

        #region Colors

        private (Color, string) GetColorAndNameForCell(Vector2Int coord)
        {
            if (IsCellOccupiedByTrainInLevel(coord))
            {
                var trainHolder = GetTrainHolder(coord);

                return (GetColorForTrain(trainHolder.trainColor), DirectionText(trainHolder.headDirection));
            }

            if (_levelEditorCurrentMode == LevelEditorModeType.TrainMode)
            {
                if (_currentTrainHolder != null)
                {
                    if (_currentTrainHolder.wagons.Any(w => w.coord == coord))
                    {
                        return (GetColorForTrain(_currentTrainHolder.trainColor),
                            DirectionText(_currentTrainHolder.headDirection));
                    }
                }
            }

            var cell = _currentLevelConfig.GetCell(coord);
            return (GetDefaultCellColor(cell.type), "");
        }

        private Color GetDefaultCellColor(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Empty: return Color.cyan;
                case CellType.Obstacle: return Color.gray;
                case CellType.NotAvailable: return Color.black;
                default: return Color.clear;
            }
        }

        private Color GetColorForTrain(TrainColor trainColor)
        {
            switch (trainColor)
            {
                case TrainColor.Blue: return Color.blue;
                case TrainColor.Red: return Color.red;
                case TrainColor.Purple: return new Color32(128, 0, 128, 255);
                case TrainColor.Orange: return new Color32(255, 165, 0, 255);
                case TrainColor.Green: return Color.green;
                default: return Color.clear;
            }
        }

        #endregion

        #region Directions

        private string DirectionText(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return "↑";
                case Direction.Down: return "↓";
                case Direction.Left: return "←";
                case Direction.Right: return "→";
                default: return string.Empty;
            }
        }

        private Vector2Int GetPossibleCoord(Vector2Int currentCoord, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return new Vector2Int(currentCoord.x, currentCoord.y + 1);
                case Direction.Down: return new Vector2Int(currentCoord.x, currentCoord.y - 1);
                case Direction.Left: return new Vector2Int(currentCoord.x - 1, currentCoord.y);
                case Direction.Right: return new Vector2Int(currentCoord.x + 1, currentCoord.y);
                default: return currentCoord;
            }
        }

        private Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return direction;
            }
        }

        private Direction VectorToDirection(Vector2Int dir)
        {
            if (dir == Vector2Int.down) return Direction.Down;
            if (dir == Vector2Int.left) return Direction.Left;
            if (dir == Vector2Int.right) return Direction.Right;
            return Direction.Up;
        }

        #endregion

        private bool IsCellOccupiedByTrainInLevel(Vector2Int coord)
        {
            foreach (var trainHolder in _currentLevelConfig.trains)
            {
                if (trainHolder.wagons.Any(w => w.coord == coord))
                {
                    return true;
                }
            }

            return false;
        }

        private TrainHolder GetTrainHolder(Vector2Int coord)
        {
            foreach (var trainHolder in _currentLevelConfig.trains)
            {
                if (trainHolder.wagons.Any(w => w.coord == coord))
                {
                    return trainHolder;
                }
            }

            return null;
        }

        private void OnCellClicked(Vector2Int coord)
        {
            if (_levelEditorCurrentMode == LevelEditorModeType.PassengerMode) return;
            if (_levelEditorCurrentMode == LevelEditorModeType.TrainMode)
            {
                if (!IsCellEmpty(coord)) return;

                if (_currentTrainHolder == null)
                {
                    var trainHolder = GetTrainHolder(coord);
                    if (trainHolder == null)
                    {
                        trainHolder = new TrainHolder();
                        trainHolder.headDirection = _currentTrainHeadDirection;
                        trainHolder.trainColor = _currentTrainColor;
                        trainHolder.wagons = new List<Wagon>();
                    }

                    _currentTrainHolder = trainHolder;
                }

                if (IsCellOccupiedByTrainInLevel(coord)) return;

                //is highlighted cell?
                if (_possiblePlacements.Count > 0 && !_possiblePlacements.Contains(coord)) return;

                if (!_currentTrainHolder.wagons.Exists(w => w.coord == coord))
                {
                    if (_currentTrainColor == _currentTrainHolder.trainColor)
                    {
                        var wagonCount = _currentTrainHolder.wagons.Count;
                        if (wagonCount == 0)
                        {
                            _currentTrainHolder.wagons.Add(new Wagon()
                            {
                                coord = coord,
                            });
                            var oppositeDirection = GetOppositeDirection(_currentTrainHolder.headDirection);
                            AddValidPlacementsToList(GetPossibleCoord(coord, oppositeDirection));
                        }
                        else
                        {
                            if (!_possiblePlacements.Contains(coord)) return;
                            _currentTrainHolder.wagons.Add(new Wagon()
                            {
                                coord = coord,
                            });
                            wagonCount++;
                            var lastWagonDir = _currentTrainHolder.wagons[wagonCount - 1].coord -
                                               _currentTrainHolder.wagons[wagonCount - 2].coord;
                            var dir = VectorToDirection(lastWagonDir);
                            CalculateThirdAndAfterWagonValidPlacementCoords(
                                _currentTrainHolder.wagons[wagonCount - 1].coord, dir);
                        }
                    }
                }
            }
            else
            {
                if (IsCellOccupiedByTrainInLevel(coord)) return;
                var cell = _currentLevelConfig.GetCell(coord);
                cell.type = _currentCellType;
                cell.isOccupied = cell.type != CellType.Empty;
            }
        }

        private void CalculateThirdAndAfterWagonValidPlacementCoords(Vector2Int coord, Direction currentDirection)
        {
            Direction oppositeDirection = GetOppositeDirection(currentDirection);

            _possiblePlacements.Clear();

            (Vector2Int firstCoord, Vector2Int secondCoord, Vector2Int thirdCoord) = CalculatePossibleCoordinates(
                coord,
                oppositeDirection
            );

            AddValidPlacementsToList(firstCoord);
            AddValidPlacementsToList(secondCoord);
            AddValidPlacementsToList(thirdCoord);
        }

        private (Vector2Int, Vector2Int, Vector2Int) CalculatePossibleCoordinates(Vector2Int coord,
            Direction oppositeDirection)
        {
            switch (oppositeDirection)
            {
                case Direction.Down:
                    return (
                        GetPossibleCoord(coord, Direction.Up),
                        GetPossibleCoord(coord, Direction.Left),
                        GetPossibleCoord(coord, Direction.Right)
                    );

                case Direction.Up:
                    return (
                        GetPossibleCoord(coord, Direction.Down),
                        GetPossibleCoord(coord, Direction.Right),
                        GetPossibleCoord(coord, Direction.Left)
                    );

                case Direction.Right:
                    return (
                        GetPossibleCoord(coord, Direction.Left),
                        GetPossibleCoord(coord, Direction.Up),
                        GetPossibleCoord(coord, Direction.Down)
                    );

                case Direction.Left:
                    return (
                        GetPossibleCoord(coord, Direction.Right),
                        GetPossibleCoord(coord, Direction.Up),
                        GetPossibleCoord(coord, Direction.Down)
                    );

                default:
                    return (Vector2Int.zero, Vector2Int.zero, Vector2Int.zero);
            }
        }

        private void AddValidPlacementsToList(Vector2Int coord)
        {
            if (IsCellOccupied(coord)) return;
            _possiblePlacements.Add(coord);
        }

        private bool IsCellOccupied(Vector2Int coord)
        {
            // If cell is not empty consider it occupied
            if (!IsCellEmpty(coord))
                return true;

            // Is it occupied by a wagon in train mode?
            if (_currentTrainHolder.wagons.Exists(w => w.coord == coord))
                return true;

            // Is there another train wagon in the level?
            if (IsCellOccupiedByTrainInLevel(coord))
                return true;

            // If nothing occupies the cell, it's free
            return false;
        }

        private bool IsCellEmpty(Vector2Int coord)
        {
            var cell = _currentLevelConfig.GetCell(coord);
            if (cell == null)
            {
                return false;
            }

            return !cell.isOccupied;
        }
    }
}