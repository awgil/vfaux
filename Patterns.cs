using Dalamud.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace vfaux;

public class Patterns
{
    public class Cell
    {
        public int ChestTL;
        public BitMask Foxes;

        public Cell(int chestTL, BitMask foxes)
        {
            ChestTL = chestTL;
            Foxes = foxes;
        }
    }

    public class Row
    {
        public int SwordsTL;
        public bool SwordsHorizontal;
        public Cell[] Cells;

        public Row(int swordsTL, bool swordsHorizontal, Cell[] cells)
        {
            SwordsTL = swordsTL;
            SwordsHorizontal = swordsHorizontal;
            Cells = cells;
        }
    }

    public class Sheet
    {
        public BitMask Blockers;
        public Row[] Rows;

        public Sheet(BitMask blockers, Row[] rows)
        {
            Blockers = blockers;
            Rows = rows;
        }
    }

    public List<Sheet> KnownPatterns = new();

    private static Sheet[] KnownBaseSheets = { // 'up' variants
        new Sheet(BitMask.Build(8, 10, 13, 26, 35), new[] { // A
            new Row(16, false, new[] {
                new Cell(0, BitMask.Build(3, 4, 12, 30)),
                new Cell(14, BitMask.Build(3, 4, 5, 12, 30)),
                new Cell(24, BitMask.Build()),
                new Cell(18, BitMask.Build()),
            }),
            new Row(15, false, new[] {
                new Cell(0, BitMask.Build(9, 12, 17, 33, 34)),
                new Cell(18, BitMask.Build(9, 17, 32, 33, 34)),
                new Cell(24, BitMask.Build()),
            }),
            new Row(21, false, new[] {
                new Cell(0, BitMask.Build(5, 15, 16, 20)),
                new Cell(18, BitMask.Build(5, 15, 16, 20)),
                new Cell(24, BitMask.Build()),
            }),
            new Row(15, true, new[] {
                new Cell(18, BitMask.Build(3, 4, 12, 30)),
                new Cell(27, BitMask.Build(3, 4, 12, 30)),
                new Cell(0, BitMask.Build()),
                new Cell(24, BitMask.Build()),
            }),
            new Row(21, true, new[] {
                new Cell(0, BitMask.Build(5, 15, 16, 20)),
                new Cell(24, BitMask.Build(5, 15, 16, 20)),
                new Cell(18, BitMask.Build()),
            }),
            new Row(14, true, new[] {
                new Cell(0, BitMask.Build(2, 11, 29, 32)),
                new Cell(24, BitMask.Build(2, 11, 29, 32)),
                new Cell(27, BitMask.Build()),
                new Cell(18, BitMask.Build()),
            }),
            new Row(18, false, new[] {
                new Cell(27, BitMask.Build(2, 11, 29, 32)),
                new Cell(16, BitMask.Build(2, 11, 29, 32, 33)),
                new Cell(22, BitMask.Build(9, 17, 33, 34)),
                new Cell(14, BitMask.Build(9, 17, 33, 34)),
                new Cell(21, BitMask.Build()),
                new Cell(0, BitMask.Build()),
                new Cell(15, BitMask.Build()),
            }),
        }),
        new Sheet(BitMask.Build(3, 13, 16, 21, 32), new[] { // B
            new Row(0, true, new[] {
                new Cell(4, BitMask.Build(15, 18, 26, 33)),
                new Cell(28, BitMask.Build(15, 18, 26, 33)),
                new Cell(24, BitMask.Build(10, 14, 20, 22)),
                new Cell(27, BitMask.Build(10, 14, 20, 22)),
                new Cell(19, BitMask.Build()),
                new Cell(22, BitMask.Build()),
                new Cell(18, BitMask.Build()),
            }),
            new Row(22, false, new[] {
                new Cell(24, BitMask.Build(15, 18, 26, 33)),
                new Cell(1, BitMask.Build(15, 18, 26, 33)),
                new Cell(19, BitMask.Build(1, 6, 8, 17)),
                new Cell(4, BitMask.Build(1, 6, 8, 17)),
                new Cell(8, BitMask.Build()),
                new Cell(0, BitMask.Build()),
                new Cell(18, BitMask.Build()),
            }),
            new Row(27, true, new[] {
                new Cell(4, BitMask.Build(1, 6, 8, 17)),
                new Cell(24, BitMask.Build(1, 6, 8, 17)),
                new Cell(8, BitMask.Build()),
                new Cell(0, BitMask.Build()),
                new Cell(19, BitMask.Build()),
                new Cell(1, BitMask.Build()),
                new Cell(18, BitMask.Build()),
            }),
            new Row(18, false, new[] {
                new Cell(0, BitMask.Build(10, 14, 20, 22)),
                new Cell(28, BitMask.Build(10, 14, 20, 22)),
                new Cell(8, BitMask.Build(2, 5, 12, 35)),
                new Cell(22, BitMask.Build(2, 5, 12, 35)),
                new Cell(4, BitMask.Build()),
                new Cell(1, BitMask.Build()),
                new Cell(27, BitMask.Build()),
            }),
            new Row(18, true, new[] {
                new Cell(0, BitMask.Build(2, 5, 12, 35)),
                new Cell(27, BitMask.Build(2, 5, 12, 35)),
                new Cell(8, BitMask.Build()),
                new Cell(4, BitMask.Build()),
                new Cell(22, BitMask.Build()),
                new Cell(28, BitMask.Build()),
                new Cell(1, BitMask.Build()),
            }),
        }),
        new Sheet(BitMask.Build(4, 7, 15, 25, 33), new[] { // С
            new Row(10, false, new[] {
                new Cell(12, BitMask.Build(0, 21, 27, 31)),
                new Cell(28, BitMask.Build(0, 21, 27, 31)),
                new Cell(20, BitMask.Build()),
                new Cell(13, BitMask.Build()),
                new Cell(2, BitMask.Build()),
            }),
            new Row(16, false, new[] {
                new Cell(12, BitMask.Build(8, 24, 34, 35)),
                new Cell(20, BitMask.Build(8, 24, 34, 35)),
                new Cell(13, BitMask.Build()),
                new Cell(2, BitMask.Build()),
            }),
            new Row(22, false, new[] {
                new Cell(2, BitMask.Build(6, 10, 17, 26)),
                new Cell(13, BitMask.Build(6, 10, 17, 26)),
                new Cell(20, BitMask.Build(1, 5, 14, 30)),
                new Cell(12, BitMask.Build(1, 5, 14, 30)),
                new Cell(10, BitMask.Build()),
            }),
            new Row(21, true, new[] {
                new Cell(2, BitMask.Build(6, 10, 17, 26)),
                new Cell(12, BitMask.Build(6, 10, 17, 26)),
                new Cell(13, BitMask.Build(8, 24, 34, 35)),
                new Cell(10, BitMask.Build(8, 24, 34, 35)),
            }),
            new Row(20, true, new[] {
                new Cell(2, BitMask.Build(1, 5, 14, 30)),
                new Cell(10, BitMask.Build(1, 5, 14, 30)),
                new Cell(12, BitMask.Build()),
            }),
            new Row(12, true, new[] {
                new Cell(2, BitMask.Build(0, 21, 27, 31)),
                new Cell(10, BitMask.Build()),
                new Cell(16, BitMask.Build()),
                new Cell(22, BitMask.Build()),
                new Cell(28, BitMask.Build()),
                new Cell(21, BitMask.Build()),
            }),
        }),
        new Sheet(BitMask.Build(7, 16, 18, 27, 32), new[] { // D
            new Row(2, true, new[] {
                new Cell(14, BitMask.Build(11, 24, 31, 34)),
                new Cell(19, BitMask.Build(11, 24, 31, 34)),
                new Cell(22, BitMask.Build()),
                new Cell(13, BitMask.Build()),
                new Cell(28, BitMask.Build()),
                new Cell(24, BitMask.Build()),
            }),
            new Row(3, true, new[] {
                new Cell(22, BitMask.Build(0, 15, 21, 35)),
                new Cell(24, BitMask.Build(0, 15, 21, 35)),
                new Cell(14, BitMask.Build()),
                new Cell(13, BitMask.Build()),
                new Cell(19, BitMask.Build()),
                new Cell(28, BitMask.Build()),
            }),
            new Row(2, false, new[] {
                new Cell(4, BitMask.Build(1, 13, 17, 26)),
                new Cell(24, BitMask.Build(1, 13, 17, 26)),
                new Cell(28, BitMask.Build()),
                new Cell(19, BitMask.Build()),
                new Cell(22, BitMask.Build()),
            }),
            new Row(8, false, new[] {
                new Cell(28, BitMask.Build(2, 3, 23, 33)),
                new Cell(24, BitMask.Build(2, 3, 23, 33)),
                new Cell(4, BitMask.Build()),
                new Cell(22, BitMask.Build()),
            }),
            new Row(22, false, new[] {
                new Cell(2, BitMask.Build(1, 13, 17, 26)),
                new Cell(4, BitMask.Build(1, 13, 17, 26)),
                new Cell(3, BitMask.Build()),
                new Cell(8, BitMask.Build()),
                new Cell(14, BitMask.Build()),
                new Cell(13, BitMask.Build()),
                new Cell(19, BitMask.Build()),
                new Cell(24, BitMask.Build()),
            }),
            new Row(13, false, new[] {
                new Cell(2, BitMask.Build(0, 15, 21, 35)),
                new Cell(22, BitMask.Build(0, 15, 21, 35)),
                new Cell(3, BitMask.Build()),
                new Cell(4, BitMask.Build()),
                new Cell(28, BitMask.Build()),
            }),
            new Row(13, true, new[] {
                new Cell(2, BitMask.Build(11, 24, 31, 34)),
                new Cell(22, BitMask.Build(11, 24, 31, 34)),
                new Cell(4, BitMask.Build(2, 3, 23, 33)),
                new Cell(28, BitMask.Build(2, 3, 23, 33)),
                new Cell(3, BitMask.Build()),
                new Cell(24, BitMask.Build()),
            }),
        }),
    };

    public Patterns()
    {
        foreach (var s in KnownBaseSheets)
        {
            KnownPatterns.Add(s);
            var r = RotateSheetLeft(s);
            KnownPatterns.Add(r);
            r = RotateSheetLeft(r);
            KnownPatterns.Add(r);
            r = RotateSheetLeft(r);
            KnownPatterns.Add(r);
        }
    }

    private static int RotateCellIndexLeft(int cell)
    {
        var x = cell % BoardState.Width;
        var y = cell / BoardState.Width;
        var yr = x;
        var xr = BoardState.Width - 1 - y;
        return yr * BoardState.Width + xr;
    }

    private static BitMask RotateCellMaskLeft(BitMask cells) => BitMask.Build(cells.SetBits().Select(RotateCellIndexLeft).ToArray());

    private static Cell RotateCellLeft(Cell cell) => new(RotateCellIndexLeft(cell.ChestTL) - 1, RotateCellMaskLeft(cell.Foxes));
    private static Row RotateRowLeft(Row row) => new(RotateCellIndexLeft(row.SwordsTL) - (row.SwordsHorizontal ? 1 : 2), !row.SwordsHorizontal, row.Cells.Select(RotateCellLeft).ToArray());
    private static Sheet RotateSheetLeft(Sheet sheet) => new(RotateCellMaskLeft(sheet.Blockers), sheet.Rows.Select(RotateRowLeft).ToArray());
}
