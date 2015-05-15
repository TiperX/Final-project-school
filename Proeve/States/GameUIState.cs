﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using E2DFramework.Graphics;

using Proeve.Entities;
using Proeve.Resources;
using Proeve.Resources.Calculations;
using Proeve.Resources.Calculations.Pathfinding;

namespace Proeve.States
{
    class GameUIState : State
    {
        private bool IsTurn { get { return Globals.multiplayerConnection.myTurn; } }
        private int selected = -1;
        private List<bool> canMove;
        private List<Point> canMoveTo;
        private List<bool> canAttack;
        private List<int> canAttackThis;

        public GameUIState()
        {
            
        }

        public override void Initialize()
        {
            canMove = new List<bool>();
            canAttack = new List<bool>();

            for (int i = 0; i < ((GameState)StateManager.GetState(1)).GetArmy().Count; i++)
            {
                canMove.Add(true);
                canAttack.Add(true);
            }
            canMove[canMove.Count - 1] = false;
            canMove[canMove.Count - 2] = false;
            canAttack[canAttack.Count - 1] = false;
            canAttack[canAttack.Count - 2] = false;

            Globals.multiplayerConnection.RecieveMove += RecievedMove;
            Globals.multiplayerConnection.RecieveFight += RecievedFight;
            Globals.multiplayerConnection.RecieveEndTurn += OtherPlayerEndedHisTurn;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsTurn)
            {
                if (Globals.mouseState.LeftButtonPressed && new Rectangle(0,0,100,100).Contains(Globals.mouseState.Position))
                {
                    //IsTurn = false;
                    //MultiplayerConnection.
                    //send endturn command
                    //no clue how this stuff works
                }

                if (selected == -1)
                {
                    if (canMove[canMove.Count-3] && Armies.army[Armies.army.Count-3].special == Character.Special.Minor)
                    {
                        if(Math.Pow(Armies.army[Armies.army.Count - 1].GridPosition.X - Armies.army[Armies.army.Count - 3].GridPosition.X,2) <= 1
                        || Math.Pow(Armies.army[Armies.army.Count - 1].GridPosition.Y - Armies.army[Armies.army.Count - 3].GridPosition.Y, 2) <= 1)
                        {
                            canMove[canMove.Count - 1] = true;
                        }
                        if(Math.Pow(Armies.army[Armies.army.Count - 2].GridPosition.X - Armies.army[Armies.army.Count - 3].GridPosition.X, 2) <= 1
                        || Math.Pow(Armies.army[Armies.army.Count - 2].GridPosition.Y - Armies.army[Armies.army.Count - 3].GridPosition.Y, 2) <= 1)
                        {
                            canMove[canMove.Count - 2] = true;
                        }
                    }
                    else
                    {
                        canMove[canMove.Count - 1] = false;
                        canMove[canMove.Count - 2] = false;
                    }
                    if (Globals.mouseState.LeftButtonPressed)
                    {
                        bool contains = false;
                        for (int i = 0; i < ((GameState)StateManager.GetState(1)).GetArmy().Count; i++)
                        {
                            if (((GameState)StateManager.GetState(1)).GetArmy()[i].Hitbox.Contains(Globals.mouseState.Position) && (canMove[i] || canAttack[i]))
                            {
                                selected = i;
                                contains = true;
                            }
                        }

                        if (!contains)
                        {
                            selected = -1;
                        }
                        else if (selected != -1)
                        {
                            if (canMove[selected])
                            {
                                int[,] level = new int[((GameState)StateManager.GetState(1)).GetLevel().GetLength(0), ((GameState)StateManager.GetState(1)).GetLevel().GetLength(1)];

                                for (int i = 0; i < level.GetLength(0); i++)
                                    for (int j = 0; j < level.GetLength(1); j++)
                                    {
                                        level[i, j] = ((GameState)StateManager.GetState(1)).GetLevel()[i, j];
                                    }

                                for (int i = 0; i < Armies.army.Count(); i++)
                                {
                                    Point gridpos = ((GameState)StateManager.GetState(1)).GetArmy()[i].GridPosition;

                                    level[gridpos.X, gridpos.Y] = 1;
                                }

                                for (int i = 0; i < Armies.opponentArmy.Count(); i++)
                                {
                                    Point gridpos = Armies.opponentArmy[i].GridPosition; // Grid.ToGridLocation(new Point((int)((GameState)StateManager.GetState(1)).GetEnemyArmy()[i].Position.X + Globals.TILE_WIDTH / 2, (int)((GameState)StateManager.GetState(1)).GetEnemyArmy()[i].Position.Y + Globals.TILE_WIDTH / 2), Globals.GridLocation, Globals.TileDimensions);
                                    level[gridpos.X, gridpos.Y] = 1;
                                }

                                Point GPos = Armies.army[selected].GridPosition;
                                level[GPos.X, GPos.Y] = 0;

                                // Uncomment to Print the grid
                                /*for (int i = 0; i < level.GetLength(0); i++)
                                {
                                    Console.WriteLine();
                                    for (int j = 0; j < level.GetLength(1); j++)
                                        Console.Write(level[i, j]);
                                }*/
                                // Uncomment to Print the grid

                                List<List<Node>> field = new List<List<Node>>();
                                for (int i = 0; i < level.GetLength(0); i++)
                                {
                                    field.Add(new List<Node>());
                                    for (int j = 0; j < level.GetLength(1); j++)
                                    {
                                        field[i].Add(new Node(i, j, level[i, j]));
                                    }
                                }

                                AStar.SetField(field);
                                AStar.Area(GPos.X, GPos.Y, Armies.army[selected].move);
                                List<Node> nodes = AStar.GetClosed();
                                canMoveTo = new List<Point>();

                                for (int i = 1; i < nodes.Count; i++)
                                    canMoveTo.Add(new Point(nodes[i].x, nodes[i].y));
                            }
                            if (canAttack[selected])
                            {
                                canAttackThis = new List<int>();

                                for (int i = 0; i < Armies.opponentArmy.Count; i++)
                                {
                                    if((Armies.opponentArmy[i].GridPosition.X == Armies.army[selected].GridPosition.X+1
                                    || Armies.opponentArmy[i].GridPosition.X == Armies.army[selected].GridPosition.X-1)
                                    && Armies.opponentArmy[i].GridPosition.Y == Armies.army[selected].GridPosition.Y)
                                    {
                                        canAttackThis.Add(i);
                                    }
                                    else if((Armies.opponentArmy[i].GridPosition.Y == Armies.army[selected].GridPosition.Y+1
                                    || Armies.opponentArmy[i].GridPosition.Y == Armies.army[selected].GridPosition.Y-1)
                                    && Armies.opponentArmy[i].GridPosition.X == Armies.army[selected].GridPosition.X)
                                    {
                                        canAttackThis.Add(i);
                                    }
                                }
                                if (!canMove[selected] && canAttackThis.Count == 0)
                                {
                                    canAttack[selected] = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Globals.mouseState.LeftButtonPressed)
                    {
                        bool contains = false;
                        if (canMoveTo != null)
                        {
                            for (int i = 0; i < canMoveTo.Count; i++)
                            {
                                Rectangle hitbox = new Rectangle(Grid.ToPixelLocation(new Point((int)canMoveTo[i].X, (int)canMoveTo[i].Y), Globals.GridLocation, Globals.TileDimensions).X, Grid.ToPixelLocation(new Point((int)canMoveTo[i].X, (int)canMoveTo[i].Y), Globals.GridLocation, Globals.TileDimensions).Y, Globals.TILE_WIDTH, Globals.TILE_HEIGHT);
                                if (hitbox.Contains(Globals.mouseState.Position))
                                {
                                    if (Armies.army[selected].special == Character.Special.Bomb)
                                    {
                                        canMove[Armies.army.Count - 3] = false;
                                    }
                                    Globals.multiplayerConnection.SendMove(selected, canMoveTo[i]);
                                    ((GameState)StateManager.GetState(1)).MoveUnit(((GameState)StateManager.GetState(1)).GetArmy()[selected], canMoveTo[i]);
                                    canMove[selected] = false;
                                    selected = -1;
                                    contains = true;
                                    canMoveTo = null;
                                    break;
                                }
                            }
                        }
                        if (canAttackThis != null)
                        {
                            for (int i = 0; i < canAttackThis.Count; i++)
                            {
                                Rectangle hitbox = Armies.opponentArmy[canAttackThis[i]].Hitbox;
                                if (hitbox.Contains(Globals.mouseState.Position))
                                {
                                    Globals.multiplayerConnection.SendFight(selected, canAttackThis[i]);
                                    ((GameState)StateManager.GetState(1)).AttackUnit(Armies.army[selected], Armies.opponentArmy[canAttackThis[i]]);

                                    canAttack[selected] = false;
                                    selected = -1;
                                    contains = true;
                                    canAttackThis = null;
                                    break;
                                }
                            }
                        }

                        if (!contains)
                        {
                            selected = -1;
                            canMoveTo = null;
                        }
                    }
                }
            }

            if (StateManager.GetState(1) is GameState)
                ((GameState)StateManager.GetState(1)).Update(gameTime);
            Globals.multiplayerConnection.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsTurn)
            {
                spriteBatch.DrawRectangle(new Rectangle(0, 0, 100, 100), Color.Aqua);
            }
            StateManager.GetState(1).Draw(spriteBatch);
            if (selected >= 0)
            {
                if (canMove[selected])
                {
                    for (int i = 0; i < canMoveTo.Count; i++)
                    {
                        spriteBatch.DrawRectangle(new Rectangle(Grid.ToPixelLocation(new Point((int)canMoveTo[i].X, (int)canMoveTo[i].Y), Globals.GridLocation, Globals.TileDimensions).X, Grid.ToPixelLocation(new Point((int)canMoveTo[i].X, (int)canMoveTo[i].Y), Globals.GridLocation, Globals.TileDimensions).Y, Globals.TILE_WIDTH, Globals.TILE_HEIGHT), Color.White);
                    }
                }
                if (canAttack[selected])
                {
                    for (int i = 0; i < canAttackThis.Count; i++)
                    {
                        spriteBatch.DrawRectangle(new Rectangle(Grid.ToPixelLocation(Armies.opponentArmy[canAttackThis[i]].GridPosition, Globals.GridLocation, Globals.TileDimensions).X, Grid.ToPixelLocation(Armies.opponentArmy[canAttackThis[i]].GridPosition, Globals.GridLocation, Globals.TileDimensions).Y, Globals.TILE_WIDTH, Globals.TILE_HEIGHT), Color.Red);
                    }
                }
            }

            spriteBatch.DrawDebugText("IsTurn: " + IsTurn, new Point(4, 4), Color.White);

            //foreach(Character c in Armies.army)
            //{
            //    spriteBatch.DrawRectangle(Globals.GridLocation.ToVector2() + c.GridPosition.ToVector2() * 82, 82, 82, Color.Blue * .5f);
            //}
        }

        private List<Character> GetArmy()
        {
            return ((GameState)StateManager.GetState(1)).GetArmy();
        }

        public List<Character> GetEnemyArmy()
        {
            return ((GameState)StateManager.GetState(1)).GetEnemyArmy(); ;
        }

        private void RecievedMove(int charIndex, Point gridLocation)
        {
            ((GameState)StateManager.GetState(1)).MoveUnit(Armies.opponentArmy[charIndex], gridLocation);
        }

        public void RecievedFight(int charIndexAttacker, int charIndexDefender)
        {
            ((GameState)StateManager.GetState(1)).AttackUnit(Armies.opponentArmy[charIndexAttacker], Armies.army[charIndexDefender]);
        }

        private void OtherPlayerEndedHisTurn()
        {
            
        }
    }
}
