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

using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ClassicUO.Configuration;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using SDL2;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using ClassicUO.Game.Managers;


namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class ServerSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x0481;
        private const ushort NORMAL_COLOR = 0x0481;

        public ServerSelectionGump() : base(0, 0)
        {
            // Background
            UIManager.Add(new LoginBackground());

            Add
            (
                new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 30, Y = 680, ButtonAction = ButtonAction.Activate
                }
            );

            Add
            (
                new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5)
                {
                    X = 980, Y = 680, ButtonAction = ButtonAction.Activate
                }
            );

            if (Client.Version >= ClientVersion.CV_500A)
            {
                ushort textColor = 0xFFFF;

                Add
                (
                    new Label(ClilocLoader.Instance.GetString(1044579), true, textColor, font: 1)
                    {
                        X = 155, Y = 70
                    }
                ); // "Select which shard to play on:"

                if (CUOEnviroment.NoServerPing == false)
                {
                    Add
                    (
                        new Label(ClilocLoader.Instance.GetString(1044577), true, textColor, font: 1)
                        {
                            X = 680, Y = 70
                        }
                    ); // "Latency:"


                }


            }
            else
            {
                ushort textColor = 0x0481;

                Add
                (
                    new Label(ResGumps.SelectWhichShardToPlayOn, false, textColor, font: 9)
                    {
                        X = 155, Y = 70
                    }
                );

                Add
                (
                    new Label(ResGumps.Latency, false, textColor, font: 9)
                    {
                        X = 600, Y = 70
                    }
                );

            }

            // Sever Scroll Area
            ScrollArea scrollArea = new ScrollArea
            (
                150,
                90,
                600,
                500,
                true
            );

            DataBox databox = new DataBox(0, 0, 1, 1);
            databox.WantUpdateSize = true;
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            scrollArea.ScissorRectangle.Y = 16;
            scrollArea.ScissorRectangle.Height = -32;

            foreach (ServerListEntry server in loginScene.Servers)
            {
                databox.Add(new ServerEntryGump(server, 5, NORMAL_COLOR, SELECTED_COLOR));
            }

            databox.ReArrangeChildren();

            Add(scrollArea);
            scrollArea.Add(databox);

            Add
              (
                 new SquareBlendControl
                 {
                     X = 193,
                     Y = 620,
                     Width = 620,
                     Height = 70,
                     Hue = 0x1AF // Cor preta (0x0000)
                 }
              );



            Add(new GumpPic(233, 620, 0x0589, 0));

            // Earth
            Add
            (
                new Button((int)Buttons.Earth, 0x15E8, 0x15EA, 0x15E9)
                {
                    X = 243,
                    Y = 630,
                    ButtonAction = ButtonAction.Activate
                }
            );

            Add
                (
                    new Label("Last server is played:", false, 0xFFFF, font: 9)
                    {
                        X = 330,
                        Y = 640
                    }
                );


            if (loginScene.Servers.Length != 0)
            {
                int index = loginScene.GetServerIndexFromSettings();

                Add
                (
                    new Label(loginScene.Servers[index].Name, false, 0xFFFF, font: 9)
                    {
                        X = 330,
                        Y = 660
                    }
                );
            }

            AcceptKeyboardInput = true;
            CanCloseWithRightClick = false;
        }

        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (buttonID >= (int) Buttons.Server)
            {
                int index = buttonID - (int) Buttons.Server;
                loginScene.SelectServer((byte) index);
            }
            else
            {
                switch ((Buttons) buttonID)
                {
                    case Buttons.Next:
                    case Buttons.Earth:

                        if (loginScene.Servers.Length != 0)
                        {
                            int index = loginScene.GetServerIndexFromSettings();

                            loginScene.SelectServer((byte) loginScene.Servers[index].Index);
                        }

                        break;

                    case Buttons.Prev:
                        loginScene.StepBack();

                        break;
                }
            }
        }

        protected override void OnControllerButtonUp(SDL.SDL_GameControllerButton button)
        {
            base.OnControllerButtonUp(button);
            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A)
            {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

                if (loginScene.Servers?.Any(s => s != null) ?? false)
                {
                    int index = loginScene.GetServerIndexFromSettings();

                    loginScene.SelectServer((byte)loginScene.Servers[index].Index);
                }
            }
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

                if (loginScene.Servers?.Any(s => s != null) ?? false)
                {
                    int index = loginScene.GetServerIndexFromSettings();

                    loginScene.SelectServer((byte) loginScene.Servers[index].Index);
                }
            }
        }

        private enum Buttons
        {
            Prev,
            Next,
            SortTimeZone,
            SortFull,
            SortConnection,
            Earth,
            Server = 99
        }


        private class ServerEntryGump : Control
        {
            private readonly int _buttonId;
            private readonly ServerListEntry _entry;
            private readonly HoveredLabel _server_ping;
            private readonly HoveredLabel _serverName;
            private uint _pingCheckTime = 0;

            public ServerEntryGump(ServerListEntry entry, byte font, ushort normal_hue, ushort selected_hue)
            {
                _entry = entry;

                _buttonId = entry.Index;

                Add
               (
                  new AlphaBlendControl
                  {
                      X = 0,
                      Width = 620,
                      Height = 30,
                      Hue = 0 // Cor preta (0x0000)
                  }
               );

                Add
                (
                    _serverName = new HoveredLabel
                    (
                        entry.Name,
                        false,
                        normal_hue,
                        selected_hue,
                        selected_hue,
                        font: font
                    )
                    {
                        X = 74,
                        AcceptMouseInput = false
                    }
                );

                Add
                (
                    _server_ping = new HoveredLabel
                    (
                        CUOEnviroment.NoServerPing ? string.Empty : "-",
                        false,
                        normal_hue,
                        selected_hue,
                        selected_hue,
                        font: font
                    )
                    {
                        X = 540,
                        AcceptMouseInput = false
                    }
                );



                AcceptMouseInput = true;
                Width = 620;
                Height = 31;

                WantUpdateSize = false;
            }

            protected override void OnMouseEnter(int x, int y)
            {
                base.OnMouseEnter(x, y);

                _serverName.IsSelected = true;
                _server_ping.IsSelected = true;
            }

            protected override void OnMouseExit(int x, int y)
            {
                base.OnMouseExit(x, y);

                _serverName.IsSelected = false;
                _server_ping.IsSelected = false;
            }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    OnButtonClick((int) Buttons.Server + _buttonId);
                }
            }

            public override void Update()
            {
                base.Update();

                if (CUOEnviroment.NoServerPing == false && _pingCheckTime < Time.Ticks)
                {
                    _pingCheckTime = Time.Ticks + 2000;
                    _entry.DoPing();

                    switch (_entry.PingStatus)
                    {
                        case IPStatus.Success:
                            _server_ping.Text = _entry.Ping == -1 ? "-" : _entry.Ping.ToString();

                            break;

                        case IPStatus.DestinationNetworkUnreachable:
                        case IPStatus.DestinationHostUnreachable:
                        case IPStatus.DestinationProtocolUnreachable:
                        case IPStatus.DestinationPortUnreachable:
                        case IPStatus.DestinationUnreachable:
                            _server_ping.Text = "unreach.";

                            break;

                        case IPStatus.TimedOut:
                            _server_ping.Text = "time out";

                            break;

                        default:
                            _server_ping.Text = $"unk. [{(int) _entry.PingStatus}]";

                            break;
                    }

                }
            }
        }
    }
}