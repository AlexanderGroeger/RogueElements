﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    /// <summary>
    /// Spawns objects of type E to IPlaceableGenContext T.
    /// Child classes offer a different way to place the list of spawns provided by Spawn.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    [Serializable]
    public abstract class BaseSpawnStep<T, E> : GenStep<T>
        where T : class, IPlaceableGenContext<E>
    {
        public IStepSpawner<T, E> Spawn;

        public BaseSpawnStep() { }
        public BaseSpawnStep(IStepSpawner<T, E> spawn)
        {
            Spawn = spawn;
        }

        public abstract void DistributeSpawns(T map, List<E> spawns);

        public override void Apply(T map)
        {
            List<E> spawns = Spawn.GetSpawns(map);

            if (spawns.Count > 0)
                DistributeSpawns(map, spawns);
        }
    }
}
