using System;

namespace vfaux;

public class BoardState
{
    [Flags]
    public enum Tile
    {
        Unknown = 0,
        Hidden = 1 << 0,
        Blocked = 1 << 1,
        Empty = 1 << 2,
        BoxTL = 1 << 3,
        BoxTR = 1 << 4,
        BoxBL = 1 << 5,
        BoxBR = 1 << 6,
        ChestTL = 1 << 7,
        ChestTR = 1 << 8,
        ChestBL = 1 << 9,
        ChestBR = 1 << 10,
        SwordsTL = 1 << 11,
        SwordsTR = 1 << 12,
        SwordsML = 1 << 13,
        SwordsMR = 1 << 14,
        SwordsBL = 1 << 15,
        SwordsBR = 1 << 16,
        Commander = 1 << 17,
        RotatedL = 1 << 18,
        RotatedR = 1 << 19,
        RotatedEither = RotatedL | RotatedR,
        Box = BoxTL | BoxTR | BoxBL | BoxBR | RotatedEither,
        Chest = ChestTL | ChestTR | ChestBL | ChestBR | RotatedEither,
        BoxChest = Box | Chest,
        Swords = SwordsTL | SwordsTR | SwordsML | SwordsMR | SwordsBL | SwordsBR | RotatedEither,
    }

    public const int Width = 6;
    public const int Height = 6;

    public Tile[] Tiles = new Tile[Width * Height];
    public BitMask Blockers;
    public int SwordsTL = -1;
    public bool SwordsHorizontal = false;
    public int BoxChestTL = -1;

    public bool Update(Tile[] tiles)
    {
        var data = AnalyzeBoard(tiles);
        if (data != null)
        {
            Tiles = tiles;
            (Blockers, SwordsTL, SwordsHorizontal, BoxChestTL) = data.Value;
            return true;
        }
        else
        {
            Plugin.Log?.Error($"Inconsistent tile pattern: {string.Join(',', tiles)}");
            Array.Fill(Tiles, Tile.Unknown);
            Blockers.Reset();
            SwordsTL = -1;
            SwordsHorizontal = false;
            BoxChestTL = -1;
            return false;
        }
    }

    private static (BitMask blockers, int swordsTL, bool swordsHoriz, int boxChestTL)? AnalyzeBoard(Tile[] tiles)
    {
        BitMask blockers = new();
        int swordsTL = -1;
        bool swordsHoriz = false;
        int boxChestTL = -1;
        for (int i = 0; i < tiles.Length; ++i)
        {
            var t = tiles[i];
            if (t.HasFlag(Tile.Blocked))
            {
                blockers.Set(i);
            }
            else if ((t & Tile.Swords) != 0)
            {
                var tl = i - TLOffsetSwords(t);
                bool horiz = (t & Tile.RotatedEither) != 0;
                if (tl > i || swordsTL != -1 && (swordsTL != tl || swordsHoriz != horiz))
                    return null;
                swordsTL = tl;
                swordsHoriz = horiz;
            }
            else if ((t & Tile.BoxChest) != 0)
            {
                var tl = i - TLOffsetBoxChest(t);
                if (tl > i || boxChestTL != -1 && boxChestTL != tl)
                    return null;
                boxChestTL = tl;
            }
        }
        return (blockers, swordsTL, swordsHoriz, boxChestTL);
    }

    private static int TLOffsetSwords(Tile t) => t switch
    {
        Tile.SwordsTL => 0,
        Tile.SwordsTR => 1,
        Tile.SwordsML => Width,
        Tile.SwordsMR => Width + 1,
        Tile.SwordsBL => Width * 2,
        Tile.SwordsBR => Width * 2 + 1,

        Tile.SwordsTL | Tile.RotatedL => Width,
        Tile.SwordsTR | Tile.RotatedL => 0,
        Tile.SwordsML | Tile.RotatedL => Width + 1,
        Tile.SwordsMR | Tile.RotatedL => 1,
        Tile.SwordsBL | Tile.RotatedL => Width + 2,
        Tile.SwordsBR | Tile.RotatedL => 2,

        Tile.SwordsTL | Tile.RotatedR => 2,
        Tile.SwordsTR | Tile.RotatedR => Width + 2,
        Tile.SwordsML | Tile.RotatedR => 1,
        Tile.SwordsMR | Tile.RotatedR => Width + 1,
        Tile.SwordsBL | Tile.RotatedR => 0,
        Tile.SwordsBR | Tile.RotatedR => Width,

        _ => -1
    };

    private static int TLOffsetBoxChest(Tile t) => t switch
    {
        Tile.BoxTL or Tile.ChestTL => 0,
        Tile.BoxTR or Tile.ChestTR => 1,
        Tile.BoxBL or Tile.ChestBL => Width,
        Tile.BoxBR or Tile.ChestBR => Width + 1,

        Tile.BoxTL | Tile.RotatedL or Tile.ChestTL | Tile.RotatedL => Width,
        Tile.BoxTR | Tile.RotatedL or Tile.ChestTR | Tile.RotatedL => 0,
        Tile.BoxBL | Tile.RotatedL or Tile.ChestBL | Tile.RotatedL => Width + 1,
        Tile.BoxBR | Tile.RotatedL or Tile.ChestBR | Tile.RotatedL => 1,

        Tile.BoxTL | Tile.RotatedR or Tile.ChestTL | Tile.RotatedR => 1,
        Tile.BoxTR | Tile.RotatedR or Tile.ChestTR | Tile.RotatedR => Width + 1,
        Tile.BoxBL | Tile.RotatedR or Tile.ChestBL | Tile.RotatedR => 0,
        Tile.BoxBR | Tile.RotatedR or Tile.ChestBR | Tile.RotatedR => Width,

        _ => -1
    };
}
