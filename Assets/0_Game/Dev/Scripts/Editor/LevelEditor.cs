using System;
using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Grid;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Train;
using _0_Game.Dev.Scripts.Passenger;
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

        #region PassengerMode

        private TrainColor _currentPassengerColor = TrainColor.Blue;
        private PassengerSide _selectedPassengerSide;
        private List<PassengerQueue> _deletedQueues = new List<PassengerQueue>();
        private Dictionary<PassengerSide, Dictionary<Vector2Int, Rect>> _passengerButtonRects =
            new Dictionary<PassengerSide, Dictionary<Vector2Int, Rect>>();

        #endregion

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
        private const float PassengerButtonHeight = 20f;
        private const float PassengerButtonWidth = 30f;
        private const float PassengerQueueOffset = 20f;

        #endregion


        [MenuItem("Tools/Level Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<LevelEditor>();
            window.titleContent = new GUIContent("Level Editor");
            window.Show();

            window._passengerButtonRects.Clear();
            foreach (PassengerSide side in Enum.GetValues(typeof(PassengerSide)))
            {
                window._passengerButtonRects[side] = new Dictionary<Vector2Int, Rect>();
            }
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
            else if (_levelEditorCurrentMode == LevelEditorModeType.PassengerMode)
            {
                DrawPassengerMode();
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

            EditorGUILayout.LabelField("Instructions:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_errorMsg, MessageType.Info, true);
            EditorGUILayout.EndVertical();
        }

        private void DrawPassengerMode()
        {
            EditorGUILayout.BeginVertical();
            _currentPassengerColor = (TrainColor)EditorGUILayout.EnumPopup("Passenger Color", _currentPassengerColor);
            EditorGUILayout.LabelField("Instructions:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click on an edge button (T, B, L, R) to place a passenger at that position.",
                MessageType.Info);
            EditorGUILayout.HelpBox(" Click the passenger button(P) for remove passenger.",
                MessageType.Info);
            var topSidePassengerCount = _currentLevelConfig.passengerQueues.Where(q => q.side == PassengerSide.Top)
                .Max(q => q.passengers.Count);

            EditorGUILayout.Space(topSidePassengerCount * PassengerQueueOffset);
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

            foreach (var sideDict in _passengerButtonRects.Values)
            {
                sideDict.Clear();
            }

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

                    var cell = _currentLevelConfig.GetCell(coordinates);

                    if (cell.type == CellType.NotAvailable &&
                        _levelEditorCurrentMode == LevelEditorModeType.PassengerMode)
                    {
                    }
                    else
                    {
                        if (GUI.Button(buttonRect, buttonText, _customButtonStyle))
                        {
                            OnCellClicked(coordinates);
                        }
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

            if (Event.current.type == EventType.Repaint)
            {
            }

            foreach (var queue in _currentLevelConfig.passengerQueues)
            {
                DrawPassengerQueue(queue);
            }

            foreach (var queue in _deletedQueues)
            {
                _currentLevelConfig.passengerQueues.Remove(queue);
            }
            _deletedQueues.Clear();

            GUI.backgroundColor = Color.white;
        }

        private void DrawPassengerQueue(PassengerQueue queue)
        {
            if (!_passengerButtonRects.ContainsKey(queue.side) ||
                !_passengerButtonRects[queue.side].ContainsKey(queue.gridPosition))
                return;

            Rect baseRect = _passengerButtonRects[queue.side][queue.gridPosition];

            Vector2 queueDirection = queue.side switch
            {
                PassengerSide.Top => new Vector2(0, -1),
                PassengerSide.Bottom => new Vector2(0, 1),
                PassengerSide.Left => new Vector2(-1, 0),
                PassengerSide.Right => new Vector2(1, 0),
                _ => Vector2.zero
            };

            for (int i = 0; i < queue.passengers.Count; i++)
            {
                var passenger = queue.passengers[i];
                float offset = (i + 1) * PassengerQueueOffset;

                Rect passengerRect = new Rect(
                    baseRect.x + queueDirection.x * offset,
                    baseRect.y + queueDirection.y * offset,
                    baseRect.width,
                    baseRect.height
                );

                GUI.backgroundColor = GetColorForTrain(passenger.color);
                if (GUI.Button(passengerRect, "P"))
                {
                    OnPassengerButtonClicked(queue, i);
                }

                //GUI.Box(passengerRect, $"P{i + 1}");
                GUI.backgroundColor = Color.white;
            }
        }
  
        private void OnPassengerButtonClicked(PassengerQueue queue, int index)
        {
            Debug.Log($"Button index {index} {queue.gridPosition}  {queue.side}");
            queue.RemovePassenger(index);
            if (queue.passengers.Count == 0)
            {
               _deletedQueues.Add(queue);
            }
        }

        private void DrawPassengerButton(int x, int y, Rect buttonRect, int gridWidth, int gridHeight)
        {
            Vector2Int gridPos = new Vector2Int(x, y);
            var cell = _currentLevelConfig.GetCell(new Vector2Int(x, y));
            if (cell != null && cell.type == CellType.Obstacle)
            {
                return;
            }

            if (cell.type == CellType.NotAvailable)
            {
                // var leftCell = _currentLevelConfig.GetCell(new Vector2Int(x - 1, y));
                //
                // if (leftCell is { type: CellType.Empty })
                // {
                //     var rect = new Rect(buttonRect.x, buttonRect.y + PassengerButtonHeight / 3 + 10,
                //         PassengerButtonHeight,
                //         PassengerButtonWidth - 10);
                //     DrawPassengerInSideButton(rect, "R", Direction.Left);
                // }
                //
                // var rightCell = _currentLevelConfig.GetCell(new Vector2Int(x + 1, y));
                //
                // if (rightCell is { type: CellType.Empty })
                // {
                //     var rect = new Rect(buttonRect.x + buttonRect.width - 15,
                //         buttonRect.y + PassengerButtonHeight / 3 + 10,
                //         PassengerButtonHeight,
                //         PassengerButtonWidth - 10);
                //     DrawPassengerInSideButton(rect, "L", Direction.Right);
                // }
                //
                // var topCell = _currentLevelConfig.GetCell(new Vector2Int(x, y + 1));
                //
                // if (topCell is { type: CellType.Empty })
                // {
                //     var rect = new Rect(buttonRect.x + PassengerButtonWidth / 3 + 10, buttonRect.y,
                //         PassengerButtonWidth - 10,
                //         PassengerButtonHeight);
                //     DrawPassengerInSideButton(rect, "B", Direction.Up);
                // }
                //
                // var bottomCell = _currentLevelConfig.GetCell(new Vector2Int(x, y - 1));
                // if (bottomCell is { type: CellType.Empty })
                // {
                //     var rect = new Rect(buttonRect.x + PassengerButtonWidth / 3 + 10, buttonRect.y + OffsetTop + 10,
                //         PassengerButtonWidth - 10,
                //         PassengerButtonHeight);
                //     DrawPassengerInSideButton(rect, "T", Direction.Down);
                // }
            }

            if (cell.type == CellType.NotAvailable)
            {
                return;
            }

            if (y == gridHeight - 1) // Top
            {
                var rect = new Rect(buttonRect.x + PassengerButtonWidth / 3, buttonRect.y - OffsetTop,
                    PassengerButtonWidth,
                    PassengerButtonHeight);
                _passengerButtonRects[PassengerSide.Top][gridPos] = rect;
                TryDrawPassengerButton(rect, "T", gridPos, PassengerSide.Top);
            }
            else if (y == 0) // Bottom
            {
                var rect = new Rect(buttonRect.x + PassengerButtonWidth / 3, buttonRect.y + OffsetBottom,
                    PassengerButtonWidth, PassengerButtonHeight);
                _passengerButtonRects[PassengerSide.Bottom][gridPos] = rect;
                TryDrawPassengerButton(rect, "B", gridPos, PassengerSide.Bottom);
            }

            if (x == gridWidth - 1) // Right edge
            {
                var rect = new Rect(buttonRect.x + OffsetSideRight, buttonRect.y + PassengerButtonHeight / 3,
                    PassengerButtonHeight,
                    PassengerButtonWidth);
                _passengerButtonRects[PassengerSide.Right][gridPos] = rect;
                TryDrawPassengerButton(rect, "R", gridPos, PassengerSide.Right);
            }
            else if (x == 0) // Left edge
            {
                var rect = new Rect(buttonRect.x - OffsetSideLeft, buttonRect.y + PassengerButtonHeight / 3,
                    PassengerButtonHeight,
                    PassengerButtonWidth);
                _passengerButtonRects[PassengerSide.Left][gridPos] = rect;
                TryDrawPassengerButton(rect, "L", gridPos, PassengerSide.Left);
            }
        }

        private void TryDrawPassengerButton(Rect rect, string label, Vector2Int gridPos, PassengerSide side)
        {
            GUI.backgroundColor = Color.white;
            if (GUI.Button(rect, label))
            {
                Debug.Log($"Button {label} clicked at {rect}  {side}");
                OnPassengerButtonClicked(gridPos, side);
            }
        }

        private void OnPassengerButtonClicked(Vector2Int gridPos, PassengerSide side)
        {
            PassengerQueue existingQueue = _currentLevelConfig.GetPassengerQueueAtPosition(gridPos, side);

            if (existingQueue != null)
            {
                existingQueue.AddPassenger(_currentPassengerColor);
            }
            else
            {
                var newQueue = new PassengerQueue()
                {
                    gridPosition = gridPos,
                    side = side
                };
                newQueue.AddPassenger(_currentPassengerColor);
                _currentLevelConfig.passengerQueues.Add(newQueue);
            }
        }

        private void DrawPassengerInSideButton(Rect rect, string label, Direction direction)
        {
            GUI.backgroundColor = Color.white;
            if (GUI.Button(rect, label))
            {
                Debug.Log($"Button {label} clicked at {rect}  {direction}");
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