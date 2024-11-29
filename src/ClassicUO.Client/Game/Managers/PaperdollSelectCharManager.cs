﻿using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassicUO.Game.Managers
{
    public class PaperdollItem
    {
        public uint Serial { get; set; }
        public Layer Layer { get; set; }
        public ushort Graphic { get; set; }
        public ushort Hue { get; set; }
        public ushort AnimID { get; set; }
        public bool IsPartialHue { get; set; }
    }

    internal class PaperdollSelectCharManager
    {
        public static PaperdollSelectCharManager Instance => instance ??= new PaperdollSelectCharManager();

        private Dictionary<string, PaperdollItem> items = new Dictionary<string, PaperdollItem>();

        private string savePath;

        private static PaperdollSelectCharManager instance;

        private PaperdollSelectCharManager()
        {
            Load();
        }

        public void AddItem(string key, Layer layer, ushort graphic, ushort hue, uint serial, ushort animID, bool isPartialHue)
        {
            if (items.ContainsKey(key))
            {
                items[key] = new PaperdollItem
                {
                    Layer = layer,
                    Graphic = graphic,
                    Hue = hue,
                    Serial = serial, 
                    AnimID = animID,
                    IsPartialHue = isPartialHue
                };
            }
            else
            {
                items.Add(key, new PaperdollItem
                {
                    Layer = layer,
                    Graphic = graphic,
                    Hue = hue,
                    Serial = serial,
                    AnimID = animID,
                    IsPartialHue = isPartialHue
                });
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(items, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                savePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles", Settings.GlobalSettings.Username, World.ServerName, World.Player.Name, "paperdollSelectCharManager.json");
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save marked tile data: {ex.Message}");
            }
        }

        public void Load()
        {
            savePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles", Settings.GlobalSettings.Username, World.ServerName, World.Player.Name, "paperdollSelectCharManager.json");
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    items = JsonSerializer.Deserialize<Dictionary<string, PaperdollItem>>(json) ?? new Dictionary<string, PaperdollItem>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load marked tile data: {ex.Message}");
                }
            }
        }
    }
}
