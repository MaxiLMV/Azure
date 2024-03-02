﻿using ProjectM;
using System.Text.RegularExpressions;
using VBuild.Core;
using VampireCommandFramework;
using VBuild.BuildingSystem;

namespace VBuild.BuildingSystem
{
    public class BuildSettings
    {
        public bool CanEditTiles { get; set; } // setting to allow moving or dismantling tiles outside of territories
        public bool BuildMode { get; set; }  // setting to spawn tiles using charm T02 on Q
        public int TileRotation { get; set; } // controls orientation of tiles placed
        public int TileModel { get; set; } // controls model of tiles placed
        public string TileSet { get; set; } // tileset of tiles to select from
        public Stack<string> LastTilesPlaced { get; set; } = new Stack<string>(); // string representation of entity identifier of last tile models placed for easy undoing
        public bool ImmortalTiles { get; set; } // setting to make tiles indestructible
        public bool MapIconTiles { get; set; } // setting to allow map icons to be placed on tiles
        public BuildSettings(bool canEditTiles, bool buildMode, int tileRotation, int tileModel, string tileSet, Stack<string> lastTilesPlaced, bool immortalTiles, bool mapIconTiles)
        {
            CanEditTiles = canEditTiles;
            BuildMode = buildMode;
            TileRotation = tileRotation;
            TileModel = tileModel;
            TileSet = tileSet;
            LastTilesPlaced = lastTilesPlaced;
            ImmortalTiles = immortalTiles;
            MapIconTiles = mapIconTiles;
        }

        
        public void AddTilePlaced(string tileRef)
        {
            if (LastTilesPlaced.Count >= 10)
            {
                LastTilesPlaced.Pop(); // Ensure we only keep the last 10
            }
            LastTilesPlaced.Push(tileRef);
        }

        public string PopLastTilePlaced()
        {
            return LastTilesPlaced.Count > 0 ? LastTilesPlaced.Pop() : null;
        }
        
    }

    
}