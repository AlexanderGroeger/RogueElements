﻿// <copyright file="IGridPathBranch.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace RogueElements
{
    public interface IGridPathBranch
    {
        RandRange RoomRatio { get; set; }

        RandRange BranchRatio { get; set; }
    }

    /// <summary>
    /// Populates the empty grid plan of a map by creating a minimum spanning tree of connected rooms and halls.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class GridPathBranch<T> : GridPathStartStepGeneric<T>, IGridPathBranch
        where T : class, IRoomGridGenContext
    {
        public GridPathBranch()
            : base()
        {
        }

        /// <summary>
        /// The percentage of total rooms in the grid plan that the step aims to fill.
        /// </summary>
        public RandRange RoomRatio { get; set; }

        /// <summary>
        /// The percent amount of branching paths the layout will have in relation to its straight paths.
        /// 0 = A layout without branches. (Worm)
        /// 50 = A layout that branches once for every two extensions. (Tree)
        /// 100 = A layout that branches once for every extension. (Branchier Tree)
        /// 200 = A layout that branches twice for every extension. (Fuzzy Worm)
        /// </summary>
        public RandRange BranchRatio { get; set; }

        /// <summary>
        /// Prevents the step from making branches in the path, even if it would fail the space-fill quota.
        /// </summary>
        public bool NoForcedBranches { get; set; }

        public static List<LocRay4> GetPossibleExpansions(GridPlan floorPlan, bool branch)
        {
            List<LocRay4> availableRays = new List<LocRay4>();
            for (int ii = 0; ii < floorPlan.RoomCount; ii++)
            {
                List<int> adjacents = floorPlan.GetAdjacentRooms(ii);
                if ((adjacents.Count <= 1) != branch)
                {
                    foreach (Dir4 dir in GetRoomExpandDirs(floorPlan, floorPlan.GetRoomPlan(ii).Bounds.Start))
                        availableRays.Add(new LocRay4(floorPlan.GetRoomPlan(ii).Bounds.Start, dir));
                }
            }

            return availableRays;
        }

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            for (int ii = 0; ii < 10; ii++)
            {
                // always clear before trying
                floorPlan.Clear();

                int roomsToOpen = floorPlan.GridWidth * floorPlan.GridHeight * this.RoomRatio.Pick(rand) / 100;
                if (roomsToOpen < 1)
                    roomsToOpen = 1;

                int addBranch = this.BranchRatio.Pick(rand);
                int roomsLeft = roomsToOpen;
                List<Loc> terminals = new List<Loc>();
                List<Loc> branchables = new List<Loc>();

                // place first room
                Loc sourceRoom = new Loc(rand.Next(floorPlan.GridWidth), rand.Next(floorPlan.GridHeight)); // randomly determine start room
                floorPlan.AddRoom(sourceRoom, this.GenericRooms.Pick(rand), this.RoomComponents.Clone());

                // add the room to a terminals list twice
                terminals.Add(sourceRoom);
                terminals.Add(sourceRoom);

                GenContextDebug.DebugProgress("Start Room");

                roomsLeft--;
                int pendingBranch = 0;
                while (roomsLeft > 0)
                {
                    // pop a random loc from the terminals list
                    Loc newTerminal = PopRandomLoc(rand, terminals);

                    // find the directions to extend to
                    List<LocRay4> availableRays = new List<LocRay4>();
                    foreach (Dir4 dir in GetRoomExpandDirs(floorPlan, newTerminal))
                        availableRays.Add(new LocRay4(newTerminal, dir));

                    if (availableRays.Count > 0)
                    {
                        // extend the path a random direction
                        LocRay4 terminalRay = availableRays[rand.Next(availableRays.Count)];
                        this.ExpandPath(rand, floorPlan, terminalRay);
                        Loc newRoomLoc = terminalRay.Traverse(1);
                        roomsLeft--;

                        // add the new terminal location to the terminals list
                        terminals.Add(newRoomLoc);
                        if (floorPlan.RoomCount > 2)
                        {
                            if (availableRays.Count > 1)
                                branchables.Add(newTerminal);

                            pendingBranch += addBranch;
                        }
                    }
                    else if (terminals.Count == 0)
                    {
                        if (this.NoForcedBranches)
                            break;
                        else
                            pendingBranch = 100;
                    }

                    while (pendingBranch >= 100 && roomsLeft > 0 && branchables.Count > 0)
                    {
                        // pop a random loc from the branchables list
                        Loc newBranch = PopRandomLoc(rand, branchables);

                        // find the directions to extend to
                        List<LocRay4> availableBranchRays = new List<LocRay4>();
                        foreach (Dir4 dir in GetRoomExpandDirs(floorPlan, newBranch))
                            availableBranchRays.Add(new LocRay4(newBranch, dir));

                        if (availableBranchRays.Count > 0)
                        {
                            // extend the path a random direction
                            LocRay4 branchRay = availableBranchRays[rand.Next(availableBranchRays.Count)];
                            this.ExpandPath(rand, floorPlan, branchRay);
                            Loc newRoomLoc = branchRay.Traverse(1);
                            roomsLeft--;

                            // add the new terminal location to the terminals list
                            terminals.Add(newRoomLoc);
                            if (availableBranchRays.Count > 1)
                                branchables.Add(newBranch);

                            pendingBranch -= 100;
                        }
                    }

                    if (terminals.Count == 0 && branchables.Count == 0)
                        break;
                }

                if (roomsLeft <= 0)
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: Fill:{1}% Branch:{2}%", this.GetType().GetFormattedTypeName(), this.RoomRatio, this.BranchRatio);
        }

        protected bool ExpandPath(IRandom rand, GridPlan floorPlan, LocRay4 chosenRay)
        {
            floorPlan.SetHall(chosenRay, this.GenericHalls.Pick(rand), this.HallComponents.Clone());
            floorPlan.AddRoom(chosenRay.Traverse(1), this.GenericRooms.Pick(rand), this.RoomComponents.Clone());

            GenContextDebug.DebugProgress("Added Path");
            return true;
        }

        /// <summary>
        /// Gets the directions a room can expand in.
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        private static IEnumerable<Dir4> GetRoomExpandDirs(GridPlan floorPlan, Loc loc)
        {
            foreach (Dir4 dir in DirExt.VALID_DIR4)
            {
                Loc endLoc = loc + dir.GetLoc();
                if ((floorPlan.Wrap || Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, endLoc))
                    && floorPlan.GetRoomIndex(endLoc) == -1)
                    yield return dir;
            }
        }

        private static Loc PopRandomLoc(IRandom rand, List<Loc> locs)
        {
            int branchIdx = rand.Next(locs.Count);
            Loc newBranch = locs[branchIdx];
            locs.RemoveAt(branchIdx);
            return newBranch;
        }
    }
}
