﻿#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Resources;
using ClassicUO.Utility;
using SDL2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Security.Cryptography;
using ClassicUO.Renderer.Arts;
using ClassicUO.Renderer;
using ClassicUO.Game.GameObjects;
using Cyotek.Drawing.BitmapFont;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using ClassicUO.Game.UI.Controls;
using static ClassicUO.Game.UI.Controls.PaperDollInteractable;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class CharacterSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0xAAF;
        private const ushort NORMAL_COLOR = 0xAAF;
        private uint _selectedCharacter;
        private ImageButton button;
        private CharacterEntryGump _characterEntryGump;
        private CharacterEntryGump _lastSelectedGumpPic;
        private static Art art { get; set; }

        public CharacterSelectionGump() : base(0, 0)
        {
            CanCloseWithRightClick = false;

            int posInList = 0;
            int yOffset = 320;
            int yBonus = 0;
            int listTitleY = 106;

            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            string lastCharName = LastCharacterManager.GetLastCharacter(LoginScene.Account, World.ServerName);
            string lastSelected = loginScene.Characters.FirstOrDefault(o => o == lastCharName);

            LockedFeatureFlags f = World.ClientLockedFeatures.Flags;
            CharacterListFlags ff = World.ClientFeatures.Flags;

            if (Client.Version >= ClientVersion.CV_6040 || Client.Version >= ClientVersion.CV_5020 && loginScene.Characters.Length > 5)
            {
                listTitleY = 96;
                yOffset = 320;
                yBonus = 45;
            }

            if (!string.IsNullOrEmpty(lastSelected))
            {
                _selectedCharacter = (uint)Array.IndexOf(loginScene.Characters, lastSelected);
            }
            else if (loginScene.Characters.Length > 0)
            {
                _selectedCharacter = 0;
            }


            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            bool unicode = isAsianLang;
            byte font = (byte)(isAsianLang ? 1 : 2);
            ushort hue = (ushort)(isAsianLang ? 0 : 0);

            Add
               (
                   new TextBox(ClilocLoader.Instance.GetString(3000050, "Character Selection"), TrueTypeLoader.EMBEDDED_FONT, 30, 300, Color.Orange, strokeEffect: true) { X = 447, Y = listTitleY, AcceptMouseInput = true }

               );


            for (int i = 0, valid = 0; i < loginScene.Characters.Length; i++)
            {
                string character = loginScene.Characters[i];
                uint bodyId = loginScene.GetCharacterBodyID(i);

                if (!string.IsNullOrEmpty(character))
                {
                    valid++;

                    if (valid > World.ClientFeatures.MaxChars)
                    {
                        break;
                    }

                    if (World.ClientLockedFeatures.Flags != 0 && !World.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SeventhCharacterSlot))
                    {
                        if (valid == 6 && !World.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SixthCharacterSlot))
                        {
                            break;
                        }
                    }

                    Add
                    (
                        _characterEntryGump = new CharacterEntryGump((uint)i, character, bodyId, SelectCharacter, LoginCharacter, SelectCharacterHover)
                        {
                            X = 30 + posInList * 150,
                            Y = yOffset + posInList * i + 3
                        },
                        1
                    );



                    posInList++;
                }
            }

            if (CanCreateChar(loginScene))
            {
                Add
                (
                    new Button((int)Buttons.New, 0x159D, 0x159F, 0x159E)
                    {
                        X = 30,
                        Y = 210 + yBonus,
                        ButtonAction = ButtonAction.Activate
                    },
                    1
                );
            }

            Add
            (
                new Button((int)Buttons.Delete, 0x159A, 0x159C, 0x159B)
                {
                    X = 940,
                    Y = 210 + yBonus,
                    ButtonAction = ButtonAction.Activate
                },
                1
            );

            Add(button = new ImageButton(
                30,
                680,
                Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_normal_prev.png"),
                Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_pressed_prev.png"),
                Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_hover_prev.png")
            ));

            button.OnButtonClick += () =>
            {
                OnButtonClick(3);
            };

            Add(button = new ImageButton(
               920,
               680,
               Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_normal_next.png"),
               Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_pressed_next.png"),
               Path.Combine(CUOEnviroment.ExecutablePath, "ExternalImages", "btn_hover_next.png")
           ));

            button.OnButtonClick += () =>
            {
                OnButtonClick(2);
            };


            AcceptKeyboardInput = true;
            ChangePage(1);
        }

        private void OnGumpPicMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtonType.Left)
            {
                // Se existir um GumpPic previamente selecionado, restaura sua opacidade
                if (_lastSelectedGumpPic != null)
                {
                    _lastSelectedGumpPic.Alpha = 1.0f; // Restaura opacidade total
                }

                // Define o novo GumpPic clicado como selecionado
                _characterEntryGump.Alpha = 0.5f; // Define opacidade reduzida (50%)
                _lastSelectedGumpPic = _characterEntryGump; // Atualiza o GumpPic selecionado globalmente
            }
        }

        private void OnGumpPicMouseEnter(object sender, EventArgs e)
        {
            _characterEntryGump.Alpha = 0.5f; // Set opacity to 50% on hover
        }

        private void OnGumpPicMouseExit(object sender, EventArgs e)
        {
            _characterEntryGump.Alpha = 1.0f; // Reset opacity to 100% when not hovered
        }

        private bool CanCreateChar(LoginScene scene)
        {
            if (scene.Characters != null)
            {
                int empty = scene.Characters.Count(string.IsNullOrEmpty);

                if (empty >= 0 && scene.Characters.Length - empty < World.ClientFeatures.MaxChars)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnControllerButtonUp(SDL.SDL_GameControllerButton button)
        {
            base.OnControllerButtonUp(button);

            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A)
            {
                LoginCharacter(_selectedCharacter);
            }
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginCharacter(_selectedCharacter);
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            switch ((Buttons)buttonID)
            {
                case Buttons.Delete:
                    DeleteCharacter(loginScene);

                    break;

                case Buttons.New when CanCreateChar(loginScene):
                    loginScene.StartCharCreation();

                    break;

                case Buttons.Next:
                    UIManager.GetGump<LoginBackground>()?.Dispose();
                    UIManager.GetGump<CharacterSelectionBackground>()?.Dispose();
                    UIManager.GetGump<SelectServerBackground>()?.Dispose();
                    LoginCharacter(_selectedCharacter);

                    break;

                case Buttons.Prev:
                    loginScene.StepBack();

                    break;
            }

            base.OnButtonClick(buttonID);
        }

        private void DeleteCharacter(LoginScene loginScene)
        {
            string charName = loginScene.Characters[_selectedCharacter];

            if (!string.IsNullOrEmpty(charName))
            {
                LoadingGump existing = Children.OfType<LoadingGump>().FirstOrDefault();

                if (existing != null)
                {
                    Remove(existing);
                }

                Add
                (
                    new LoadingGump
                    (
                        string.Format(ResGumps.PermanentlyDelete0, charName),
                        LoginButtons.OK | LoginButtons.Cancel,
                        buttonID =>
                        {
                            if (buttonID == (int)LoginButtons.OK)
                            {
                                loginScene.DeleteCharacter(_selectedCharacter);
                            }
                            else
                            {
                                ChangePage(1);
                            }
                        }
                    ),
                    2
                );

                ChangePage(2);
            }
        }

        private void SelectCharacter(uint index)
        {
            _selectedCharacter = index;

            foreach (CharacterEntryGump characterGump in FindControls<CharacterEntryGump>())
            {
                characterGump.Hue = characterGump.CharacterIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
            }
        }

        private void SelectCharacterHover(uint index)
        {
            _selectedCharacter = index;

            foreach (CharacterEntryGump characterGump in FindControls<CharacterEntryGump>())
            {
                characterGump.Hue = characterGump.CharacterIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
                characterGump.Alpha = 0.5f; // Set opacity to 50% on hover
            }
        }

        private void SelectCharacterUnHover(uint index)
        {
            _selectedCharacter = index;

            foreach (CharacterEntryGump characterGump in FindControls<CharacterEntryGump>())
            {
                characterGump.Hue = characterGump.CharacterIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
                characterGump.Alpha = 0.5f; // Set opacity to 50% on hover
            }
        }

        private void LoginCharacter(uint index)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (loginScene.Characters != null && loginScene.Characters.Length > index && !string.IsNullOrEmpty(loginScene.Characters[index]))
            {
                UIManager.GetGump<LoginBackground>()?.Dispose();
                UIManager.GetGump<CharacterSelectionBackground>()?.Dispose();
                UIManager.GetGump<SelectServerBackground>()?.Dispose();
                loginScene.SelectCharacter(index);
            }
        }

        private enum Buttons
        {
            New,
            Delete,
            Next,
            Prev
        }

        private class CharacterEntryGump : Control
        {
            private readonly TextBox _label;
            private readonly Action<uint> _loginFn;
            private readonly Action<uint> _selectedFn;
            private readonly Action<uint> _hoverFn;
            private readonly uint _bodyID;
            private static Art art { get; set; }
            private static PlayerMobile _character;
            private PaperDollInteractable _paperDoll;
            private readonly string savePath;

            public Dictionary<string, PaperdollItem> Load()
            {
                if (File.Exists(savePath))
                {
                    try
                    {
                        string json = File.ReadAllText(savePath);
                        return JsonSerializer.Deserialize<Dictionary<string, PaperdollItem>>(json) ?? new Dictionary<string, PaperdollItem>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load marked tile data: {ex.Message}");
                        return null;
                    }
                }
                return null;
            }


            public CharacterEntryGump(uint index, string character, uint bodyID, Action<uint> selectedFn, Action<uint> loginFn, Action<uint> hoverFn)
            {
                CharacterIndex = index;
                _bodyID = bodyID;
                _selectedFn = selectedFn;
                _hoverFn = hoverFn;
                _loginFn = loginFn;
                savePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles", Settings.GlobalSettings.Username, World.ServerName, character, "paperdollSelectCharManager.json");
                var items = Load();


                // Bg
                Add(new GumpPic(0, 0, 0x000C, 0) { IsPartialHue = true }.ScaleWidthAndHeight(Scale).SetInternalScale(Scale));

                Mobile mobiles = World.Mobiles.Get(LocalSerial);

                if (items != null && savePath != null)
                {
                    var customLayerOrder = new Dictionary<Layer, int>
                    {
                    { Layer.Helmet, 7 },
                    { Layer.Robe, 6},
                    { Layer.Hair, 5 },
                    { Layer.Beard, 4 },
                    { Layer.Waist, 3 },
                    { Layer.OneHanded, 2},
                    { Layer.TwoHanded, 2 },
                    { Layer.Talisman, 1 }
                    };

                    foreach (var item in items.Values
                        .OrderBy(i => customLayerOrder.ContainsKey(i.Layer) ? customLayerOrder[i.Layer] : 0)
                        .ThenBy(i => i.Layer))
                    {
                        if (item.Graphic > 0 && item.Layer != Layer.Bracelet || item.Graphic > 0 && item.Layer != Layer.Ring || item.Graphic > 0 && item.Layer != Layer.Backpack)
                        {
                            ushort id = GetAnimID(
                                0x000C,
                                item.AnimID,
                                false
                            );

                            Add(new GumpPicEquipment(
                               item.Serial,
                               0,
                               0,
                               id,
                               (ushort)(item.Hue & 0xFFFF),
                                item.Layer
                            )
                            {
                                AcceptMouseInput = true,

                                IsPartialHue = item.IsPartialHue,
                                CanLift = World.InGame
                                   && !World.Player.IsDead
                                   && LocalSerial == World.Player,
                            }.ScaleWidthAndHeight(Scale).SetInternalScale(InternalScale));
                        }
                    }
                }
                else
                {
                    Add(new GumpPic(1, 1, 0xC4E9, 0));
                    Add(new GumpPic(1, 1, 0xC502, 0));
                    Add(new GumpPic(1, 1, 0xC4FE, 0));
                    Add(new GumpPic(1, 1, 0xC530, 0));
                }

                // Char Name


                Add
               (
                   _label = new TextBox(character, TrueTypeLoader.EMBEDDED_FONT, 16, 190, Color.Orange, align: TextHorizontalAlignment.Center, strokeEffect: true) { AcceptMouseInput = true }

               );

                AcceptMouseInput = true;
            }

            public uint CharacterIndex { get; }

            public ushort Hue
            {
                get => (ushort)_label.Hue;
                set => _label.Hue = (ushort)value;
            }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _loginFn(CharacterIndex);

                    return true;
                }

                return false;
            }


            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _selectedFn(CharacterIndex);
                }
            }

            protected override void OnMouseOver(int x, int y)
            {
                _hoverFn(CharacterIndex);
            }

            protected override void OnMouseExit(int x, int y)
            {
                _hoverFn(CharacterIndex);
            }

        }
    }
}