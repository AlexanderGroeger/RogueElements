﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    [Serializable]
    public class SpecificTilesStep<T> : GenStep<T> where T : class, ITiledGenContext
    {
        public Loc Offset;

        public ITile[][] Tiles;

        public SpecificTilesStep() { }

        public SpecificTilesStep(Loc offset) { Offset = offset; }

        public override void Apply(T map)
        {
            //initialize map array to empty
            //set default map values
            for (int xx = 0; xx < Tiles.Length; xx++)
            {
                for (int yy = 0; yy < Tiles[0].Length; yy++)
                    map.Tiles[Offset.X + xx][Offset.Y + yy] = Tiles[xx][yy].Copy();
            }
        }

    }
}
