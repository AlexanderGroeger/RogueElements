﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueElements.Examples.Ex3_Grid
{
    public static class Example3
    {
        public static void Run()
        {
            string title = "3: A Map made with Rooms and Halls arranged in a grid.";

            MapGen<MapGenContext> layout = new MapGen<MapGenContext>();

            //Initialize a 6x4 grid of 10x10 cells.
            InitGridPlanStep<MapGenContext> startGen = new InitGridPlanStep<MapGenContext>();
            startGen.CellX = 6;
            startGen.CellY = 4;

            startGen.CellWidth = 10;
            startGen.CellHeight = 10;
            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(-4, startGen));



            //Create a path that is composed of a ring around the edge
            GridPathCircle<MapGenContext> path = new GridPathCircle<MapGenContext>();
            path.CircleRoomRatio = new RandRange(80);
            path.Paths = new RandRange(3);

            SpawnList<RoomGen<MapGenContext>> genericRooms = new SpawnList<RoomGen<MapGenContext>>();
            //cross
            genericRooms.Add(new RoomGenSquare<MapGenContext>(new RandRange(4, 8), new RandRange(4, 8)));
            //round
            genericRooms.Add(new RoomGenRound<MapGenContext>(new RandRange(5, 9), new RandRange(5, 9)));
            path.GenericRooms = genericRooms;

            SpawnList<PermissiveRoomGen<MapGenContext>> genericHalls = new SpawnList<PermissiveRoomGen<MapGenContext>>();
            genericHalls.Add(new RoomGenAngledHall<MapGenContext>(50));
            path.GenericHalls = genericHalls;

            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(-4, path));



            //Output the rooms into a FloorPlan
            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(-2, new DrawGridToFloorStep<MapGenContext>()));




            //Draw the rooms of the FloorPlan onto the tiled map, with 1 TILE padded on each side
            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(0, new DrawFloorToTileStep<MapGenContext>(1)));




            //Run the generator and print
            MapGenContext context = layout.GenMap(MathUtils.Rand.NextUInt64());
            Print(context.Map, title);
        }


        public static void Print(Map map, string title)
        {
            int oldLeft = Console.CursorLeft;
            int oldTop = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            StringBuilder topString = new StringBuilder("");
            string turnString = title;
            topString.Append(String.Format("{0,-82}", turnString));
            topString.Append('\n');
            for (int i = 0; i < map.Width + 1; i++)
                topString.Append("=");
            topString.Append('\n');

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Loc loc = new Loc(x, y);
                    char tileChar = ' ';
                    Tile tile = map.Tiles[x][y];
                    if (tile.ID <= 0)//wall
                        tileChar = '#';
                    else if (tile.ID == 1)//floor
                        tileChar = '.';
                    else
                        tileChar = '?';
                    topString.Append(tileChar);
                }
                topString.Append('\n');
            }
            Console.Write(topString.ToString());
        }
    }
}
