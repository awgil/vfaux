using System.Collections.Generic;
using System.Linq;

namespace vfaux;

public class Solver
{
    // special scores
    public const int ConfirmedSword = -2;
    public const int ConfirmedBoxChest = -3;
    public const int PotentialFox = -4;

    public Patterns PatternDB = new();
    public bool FindSwordsFirst = false;

    public int[] Solve(BoardState board)
    {
        var result = new int[BoardState.Width * BoardState.Height];
        var sheet = MatchingSheet(board);
        if (sheet != null)
        {
            HashSet<(int, bool)> potentialSwords = new();
            HashSet<int> potentialBoxes = new();
            HashSet<ulong> potentialFoxes = new();
            foreach (var (r, c) in MatchingCells(board, sheet))
            {
                potentialSwords.Add((r.SwordsTL, r.SwordsHorizontal));
                potentialBoxes.Add(c.ChestTL);
                potentialFoxes.Add(c.Foxes.Raw);
            }

            var swordsScore = potentialSwords.Count == 1 ? ConfirmedSword : 1;
            var boxScore = potentialBoxes.Count == 1 ? ConfirmedBoxChest : (FindSwordsFirst && potentialSwords.Count > 1) ? 0 : 1;
            foreach (var (tl, h) in potentialSwords)
                foreach (var i in SwordIndices(tl, h))
                    result[i] += swordsScore;
            foreach (var tl in potentialBoxes)
                foreach (var i in BoxChestIndices(tl))
                    result[i] += boxScore;

            if (potentialFoxes.Count == 1)
            {
                var foxes = new BitMask(potentialFoxes.First());
                foreach (var f in foxes.SetBits())
                    result[f] = PotentialFox;
            }
        }
        return result;
    }

    public bool MatchesSheet(BoardState board, Patterns.Sheet sheet) => sheet.Blockers.Raw == board.Blockers.Raw;
    public bool MatchesRow(BoardState board, Patterns.Row row) => board.SwordsTL != -1
        ? (board.SwordsTL == row.SwordsTL && board.SwordsHorizontal == row.SwordsHorizontal)
        : AllHidden(SwordIndices(row.SwordsTL, row.SwordsHorizontal), board);
    public bool MatchesCell(BoardState board, Patterns.Cell cell) => board.BoxChestTL != -1
        ? board.BoxChestTL == cell.ChestTL
        : AllHidden(BoxChestIndices(cell.ChestTL), board);

    public Patterns.Sheet? MatchingSheet(BoardState board) => PatternDB.KnownPatterns.Find(s => MatchesSheet(board, s));
    public IEnumerable<Patterns.Row> MatchingRows(BoardState board, Patterns.Sheet sheet) => sheet.Rows.Where(r => MatchesRow(board, r));
    public IEnumerable<Patterns.Cell> MatchingCells(BoardState board, Patterns.Row row) => row.Cells.Where(c => MatchesCell(board, c));

    public IEnumerable<(Patterns.Row row, Patterns.Cell cell)> MatchingCells(BoardState board, Patterns.Sheet sheet)
    {
        foreach (var r in MatchingRows(board, sheet))
            foreach (var c in MatchingCells(board, r))
                yield return (r, c);
    }

    public static IEnumerable<int> RectIndices(int tl, int w, int h)
    {
        int sx = tl % BoardState.Width, sy = tl / BoardState.Width;
        for (int y = 0; y < h; ++y)
            for (int x = 0; x < w; ++x)
                yield return (sy + y) * BoardState.Width + (sx + x);
    }
    public static IEnumerable<int> SwordIndices(int tl, bool horiz) => horiz ? RectIndices(tl, 3, 2) : RectIndices(tl, 2, 3);
    public static IEnumerable<int> BoxChestIndices(int tl) => RectIndices(tl, 2, 2);

    private bool AllHidden(IEnumerable<int> indices, BoardState board) => indices.All(i => board.Tiles[i] == BoardState.Tile.Hidden);
}
