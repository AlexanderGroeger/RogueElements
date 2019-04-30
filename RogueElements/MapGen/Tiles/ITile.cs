﻿using System;

namespace RogueElements
{
    public interface ITile
    {
        bool TileEquivalent(ITile other);

        /// <summary>
        /// Creates a copy of the object, to be placed in the generated layout.
        /// </summary>
        /// <returns></returns>
        ITile Copy();
    }
}
