﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    /// <summary>
    /// Generates a list of spawnables to be placed in a IGenContext. This class only computes what to spawn, but not where to spawn it.
    /// </summary>
    /// <typeparam name="T">The IGenContext to place the spawns in.</typeparam>
    /// <typeparam name="E">The type of the spawn to place in IGenContext</typeparam>
    public interface IStepSpawner<T, E> where T : IGenContext
    {
        List<E> GetSpawns(T map);

    }
}
