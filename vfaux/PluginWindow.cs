using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace vfaux;

internal class PluginWindow : Window, IDisposable
{
    private BoardState _gameBoard;
    private BoardState _simBoard = new();
    private Patterns.Sheet? _simSheet;
    private Patterns.Row? _simRow;
    private Patterns.Cell? _simCell;
    private int _simFox = -1;
    private Solver _solver;

    private const int BoardSize = 150;
    private const int CellSize = 20;

    public PluginWindow(BoardState gameBoard, Solver solver) : base("Easier Faux Hollows")
    {
        Size = new Vector2(1200, 1000);
        SizeCondition = ImGuiCond.Once;
        _gameBoard = gameBoard;
        _solver = solver;
    }

    public void Dispose()
    {

    }

    public override void Draw()
    {
        ImGui.Checkbox("Solver strategy: always find swords first", ref _solver.FindSwordsFirst);

        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Current game board"))
            {
                DrawBoardTab(_gameBoard, false);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Debug board"))
            {
                if (ImGui.Button("Randomize!"))
                {
                    var rng = new Random();
                    _simSheet = _solver.PatternDB.KnownPatterns[rng.Next(_solver.PatternDB.KnownPatterns.Count)];
                    _simRow = _simSheet.Rows[rng.Next(_simSheet.Rows.Length)];
                    _simCell = _simRow.Cells[rng.Next(_simRow.Cells.Length)];
                    _simFox = _simCell.Foxes.SetBits().Skip(rng.Next(_simCell.Foxes.NumSetBits())).FirstOrDefault(-1);
                    SimBoardReset();
                }
                ImGui.SameLine();
                if (ImGui.Button("Reset tiles"))
                {
                    SimBoardReset();
                }
                DrawBoardTab(_simBoard, true);
                ImGui.EndTabItem();
            }
            for (int i = 0; i < _solver.PatternDB.KnownPatterns.Count; ++i)
            {
                if (ImGui.BeginTabItem($"P{i}"))
                {
                    DrawSheet(_solver.PatternDB.KnownPatterns[i], null);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawBoardTab(BoardState board, bool interactable)
    {
        DrawBoard(board, interactable);

        var sheet = _solver.MatchingSheet(board);
        if (sheet == null)
        {
            ImGui.TextUnformatted("Pattern not found!");
        }
        else
        {
            ImGui.TextUnformatted($"Current pattern: {_solver.PatternDB.KnownPatterns.IndexOf(sheet)}");
            DrawSheet(sheet, board);
        }
    }

    private void DrawBoard(BoardState board, bool interactable)
    {
        int tileIndex = 0;
        var solution = _solver.Solve(board);
        var bestScore = solution.Max();
        if (bestScore == 0)
            bestScore = -1;
        for (int y = 0; y < BoardState.Height; ++y)
        {
            for (int x = 0; x < BoardState.Width; ++x)
            {
                var t = solution[tileIndex];
                if (ImGui.Button($"{board.Tiles[tileIndex]} ({t}{(t == bestScore ? "*" : "")})###{x}x{y}", new(100, 0)) && interactable)
                {
                    SimBoardOpen(x, y);
                }
                ImGui.SameLine();
                ++tileIndex;
            }
            ImGui.NewLine();
        }
    }

    private void DrawSheet(Patterns.Sheet sheet, BoardState? board)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var wmin = ImGui.GetWindowPos();
        var wmax = wmin + ImGui.GetWindowSize();
        ImGui.GetWindowDrawList().PushClipRect(Vector2.Max(cursor, wmin), wmax);

        var rowX = cursor.X;
        foreach (var row in sheet.Rows)
        {
            bool matchRow = board == null || _solver.MatchesRow(board, row);
            foreach (var cell in row.Cells)
            {
                bool matchCell = board == null || matchRow && _solver.MatchesCell(board, cell);

                float alpha = matchCell ? 1 : 0.3f;
                var blockerColor = Color(128, 128, 128, alpha);
                var swordColor = Color(31, 174, 186, alpha);
                var boxColor = Color(180, 173, 44, alpha);
                var foxColor = Color(32, 143, 46, alpha);
                var borderColor = Color(255, 255, 255, alpha);

                uint[] colors = new uint[BoardState.Width * BoardState.Height];
                Action<int, uint> addColor = (index, color) => colors[index] = colors[index] == 0 ? color : Color(255, 0, 0, alpha);
                foreach (var idx in sheet.Blockers.SetBits())
                    addColor(idx, blockerColor);
                foreach (var idx in Solver.SwordIndices(row.SwordsTL, row.SwordsHorizontal))
                    addColor(idx, swordColor);
                foreach (var idx in Solver.BoxChestIndices(cell.ChestTL))
                    addColor(idx, boxColor);
                foreach (var idx in cell.Foxes.SetBits())
                    addColor(idx, foxColor);

                if (DrawBoard(cursor, colors, borderColor))
                {
                    _simSheet = sheet;
                    _simRow = row;
                    _simCell = cell;
                    _simFox = _simCell.Foxes.SetBits().Skip(new Random().Next(_simCell.Foxes.NumSetBits())).FirstOrDefault(-1);
                    SimBoardReset();
                }
                cursor.X += BoardSize;
            }
            cursor.Y += BoardSize;
            cursor.X = rowX;
        }

        ImGui.SetCursorScreenPos(cursor);
        ImGui.GetWindowDrawList().PopClipRect();
    }

    // returns whether board was double-clicked
    private bool DrawBoard(Vector2 cursor, uint[] colors, uint borderColor)
    {
        var dl = ImGui.GetWindowDrawList();
        for (int y = 0; y < BoardState.Height; ++y)
        {
            for (int x = 0; x < BoardState.Width; ++x)
            {
                var ctl = cursor + CellSize * new Vector2(x, y);
                dl.AddRectFilled(ctl, ctl + new Vector2(CellSize), colors[BoardState.Width * y + x]);
            }
        }
        for (int x = 1; x < BoardState.Width; ++x)
        {
            dl.AddRect(cursor + new Vector2(CellSize * x, 0), cursor + new Vector2(CellSize * x, CellSize * BoardState.Width), borderColor, 0, ImDrawFlags.None, 1);
            dl.AddRect(cursor + new Vector2(0, CellSize * x), cursor + new Vector2(CellSize * BoardState.Width, CellSize * x), borderColor, 0, ImDrawFlags.None, 1);
        }
        var br = cursor + new Vector2(CellSize * BoardState.Width);
        dl.AddRect(cursor, br, borderColor, 0, ImDrawFlags.None, 2); // border

        return ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ImGui.IsMouseHoveringRect(cursor, br);
    }

    private uint Color(int r, int g, int b, float a = 1.0f) => ((uint)(a * 255) << 24) | ((uint)(b * a) << 16) | ((uint)(g * a) << 8) | ((uint)(r * a));

    private void SimBoardReset()
    {
        var tiles = new BoardState.Tile[BoardState.Width * BoardState.Height];
        Array.Fill(tiles, BoardState.Tile.Hidden);
        if (_simSheet != null)
            foreach (var b in _simSheet.Blockers.SetBits())
                tiles[b] = BoardState.Tile.Blocked;
        _simBoard.Update(tiles);
    }

    private void SimBoardOpen(int x, int y)
    {
        var index = y * BoardState.Width + x;
        var tile = BoardState.Tile.Unknown;
        if (_simSheet != null && _simRow != null && _simCell != null)
        {
            if (_simSheet.Blockers[index])
            {
                tile = BoardState.Tile.Blocked;
            }
            else if (Solver.SwordIndices(_simRow.SwordsTL, _simRow.SwordsHorizontal).Contains(index))
            {
                tile = (index - _simRow.SwordsTL, _simRow.SwordsHorizontal) switch
                {
                    (0, false) => BoardState.Tile.SwordsTL,
                    (1, false) => BoardState.Tile.SwordsTR,
                    (6, false) => BoardState.Tile.SwordsML,
                    (7, false) => BoardState.Tile.SwordsMR,
                    (12, false) => BoardState.Tile.SwordsBL,
                    (13, false) => BoardState.Tile.SwordsBR,
                    (0, true) => BoardState.Tile.SwordsTR | BoardState.Tile.RotatedL,
                    (1, true) => BoardState.Tile.SwordsMR | BoardState.Tile.RotatedL,
                    (2, true) => BoardState.Tile.SwordsBR | BoardState.Tile.RotatedL,
                    (6, true) => BoardState.Tile.SwordsTL | BoardState.Tile.RotatedL,
                    (7, true) => BoardState.Tile.SwordsML | BoardState.Tile.RotatedL,
                    (8, true) => BoardState.Tile.SwordsBL | BoardState.Tile.RotatedL,
                    _ => BoardState.Tile.Unknown
                };
            }
            else if (Solver.BoxChestIndices(_simCell.ChestTL).Contains(index))
            {
                tile = (index - _simCell.ChestTL) switch
                {
                    0 => BoardState.Tile.BoxTL,
                    1 => BoardState.Tile.BoxTR,
                    6 => BoardState.Tile.BoxBL,
                    7 => BoardState.Tile.BoxBR,
                    _ => BoardState.Tile.Unknown
                };
            }
            else if (index == _simFox)
            {
                tile = BoardState.Tile.Commander;
            }
            else
            {
                tile = BoardState.Tile.Empty;
            }
        }
        var tiles = (BoardState.Tile[])_simBoard.Tiles.Clone();
        tiles[index] = tile;
        _simBoard.Update(tiles);
    }
}
