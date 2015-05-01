﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using E2DFramework.Graphics;

using Proeve.UI;
using Proeve.Entities;
using Proeve.Resources;
using Proeve.Resources.Calculations;

namespace Proeve.States
{
    class ArmyEditorState : State
    {
        private bool drag;
        private int dragIndex;
        private E2DTexture background;

        private Point gridLocation;
        private int gridWidth, gridHeight;
        private Rectangle GridArea { get { return new Rectangle(gridLocation.X, gridLocation.Y, gridWidth * Globals.TILE_WIDTH, gridHeight * Globals.TILE_HEIGHT); } }

        public ArmyEditorState()
        {
            
        }

        public override void Initialize()
        {
            dragIndex = -1;
            drag = false;

            gridWidth = Globals.GRID_WIDTH;
            gridHeight = 3;

            gridLocation = new Point(Globals.GridLocation.X, Globals.GridLocation.Y + Globals.TILE_HEIGHT * 5);

            background = ArtAssets.editorBackground;

            buttons.Add(new Button(ArtAssets.TestButton, 24, 24));
            buttons[0].ClickEvent += Ready;

            Armies.army = new List<Character>();
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Marshal));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.General));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.General));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Majoor));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Majoor));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Captain));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Captain));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Special));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Bomb));
            Armies.army.Add(Armies.GetCharacter(Armies.CharacterRanks.Bomb));

            Armies.army[0].Position = Grid.ToPixelLocation(new Point(0, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[1].Position = Grid.ToPixelLocation(new Point(1, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[2].Position = Grid.ToPixelLocation(new Point(2, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[3].Position = Grid.ToPixelLocation(new Point(3, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[4].Position = Grid.ToPixelLocation(new Point(4, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[5].Position = Grid.ToPixelLocation(new Point(5, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[6].Position = Grid.ToPixelLocation(new Point(6, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[7].Position = Grid.ToPixelLocation(new Point(7, 0), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[8].Position = Grid.ToPixelLocation(new Point(0, 1), gridLocation, Globals.TileDimensions).ToVector2();
            Armies.army[9].Position = Grid.ToPixelLocation(new Point(1, 1), gridLocation, Globals.TileDimensions).ToVector2();
        }

        private void Ready()
        {
            ((GameState)StateManager.GetState(1)).SetArmy(Armies.army);

            StateManager.ChangeState(Settings.STATES.MatchFinder);
        }

        public override void Update(GameTime gameTime)
        {
            if (Globals.mouseState.LeftButtonPressed)
                foreach(Character c in Armies.army)
                    if (c.Hitbox.Contains(Globals.mouseState.Position))
                    {
                        dragIndex = Armies.army.IndexOf(c);
                        drag = true;
                    }

            if (Globals.mouseState.LeftButtonReleased && drag)
            {
                Vector2 mouseGridPosition = (Grid.ToGridLocation(Globals.mouseState.Position.ToPoint(), gridLocation, Globals.TileDimensions) * Globals.TileDimensions + gridLocation).ToVector2();

                if (GridArea.Contains(mouseGridPosition))
                {
                    bool overlap = false;

                    foreach (Character c in Armies.army)
                        if (c.Position == mouseGridPosition)
                        {
                            overlap = true;

                            // Swap drag Character with overlap character
                            c.Position = Armies.army[dragIndex].Position;
                            Armies.army[dragIndex].Position = mouseGridPosition;

                            break;
                        }

                    if (!overlap)
                        Armies.army[dragIndex].Position = mouseGridPosition;
                }

                drag = false;
                dragIndex = -1;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawE2DTexture(background, Vector2.Zero);

            foreach(Character c in Armies.army)
            {
                if (dragIndex != Armies.army.IndexOf(c))
                    c.sprite.Draw(spriteBatch);
                else
                    spriteBatch.DrawSprite(c.sprite, Globals.mouseState.Position - new Vector2(41, 41));
            }
        }
    }
}
