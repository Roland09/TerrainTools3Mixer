﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEngine.TerrainTools;
using static UnityEngine.TerrainTools.PaintContext;
#else
using UnityEngine.Experimental.TerrainAPI;
using static UnityEngine.Experimental.TerrainAPI.PaintContext;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    public class BrushUndo : MonoBehaviour
    {
        internal enum ToolAction
        {
            None = 0,
            PaintHeightmap = 1 << 0,
            PaintTexture = 1 << 1,
            PaintHoles = 1 << 2
        }

        // This maintains the list of terrains we have touched in the current operation (and the current operation identifier, as an undo group)
        // We track this to have good cross-tile undo support: each modified tile should be added, at most, ONCE within a single operation
        private static int s_CurrentOperationUndoGroup = -1;
        private static List<UnityEngine.Object> s_CurrentOperationUndoStack = new List<UnityEngine.Object>();
        static event Action<ITerrainInfo, ToolAction, string /*editorUndoName*/> onTerrainTileBeforePaint;

        static BrushUndo()
        {
            onTerrainTileBeforePaint += (tile, action, editorUndoName) =>
            {
                // if we are in a new undo group (new operation) then start with an empty list
                if (Undo.GetCurrentGroup() != s_CurrentOperationUndoGroup)
                {
                    s_CurrentOperationUndoGroup = Undo.GetCurrentGroup();
                    s_CurrentOperationUndoStack.Clear();
                }

                if (string.IsNullOrEmpty(editorUndoName))
                    return;

                if (!s_CurrentOperationUndoStack.Contains(tile.terrain))
                {
                    s_CurrentOperationUndoStack.Add(tile.terrain);

                    var undoObjects = new List<UnityEngine.Object>();
                    undoObjects.Add(tile.terrain.terrainData);

                    // special handling for texture painting
                    if (0 != (action & ToolAction.PaintTexture))
                        undoObjects.AddRange(tile.terrain.terrainData.alphamapTextures);

                    Undo.RegisterCompleteObjectUndo(undoObjects.ToArray(), editorUndoName);
                }
            };
        }

        public static void RegisterUndo( Terrain terrain, PaintContext paintContext, string editorUndoName)
        {
            // register all affected terrains
            for( int i=0; i < paintContext.terrainCount; i++)
            {
                ITerrainInfo terrainInfo = TerrainTile.Make(
                   paintContext.GetTerrain(i),
                   paintContext.pixelRect.x,
                   paintContext.pixelRect.y,
                   paintContext.pixelRect,
                   paintContext.destinationRenderTexture.width,
                   paintContext.destinationRenderTexture.height);

                // using custom undo including texture, otherwise painting wouldn't be registered
                // might as well change the code in the onTerrainTileBeforePaint event handler
                //onTerrainTileBeforePaint?.Invoke(terrainInfo, ToolAction.PaintHeightmap, editorUndoName);
                onTerrainTileBeforePaint?.Invoke(terrainInfo, ToolAction.PaintTexture, editorUndoName);
            }

        }

        
        public class TerrainTile : ITerrainInfo
        {
            public Terrain terrain;                 // the terrain object for this tile
            public Vector2Int tileOriginPixels;     // coordinates of this terrain tile in originTerrain target texture pixels

            public RectInt clippedTerrainPixels;    // the tile pixels touched by this PaintContext (in terrain-local target texture pixels)
            public RectInt clippedPCPixels;         // the tile pixels touched by this PaintContext (in PaintContext/source/destRenderTexture pixels)

            public object userData;                 // user data stash
            public bool gatherEnable;                 // user controls for read/write
            public bool scatterEnable;

            public RectInt paddedTerrainPixels => throw new NotImplementedException();

            public RectInt paddedPCPixels => throw new NotImplementedException();

            Terrain ITerrainInfo.terrain { get { return terrain; } }
            RectInt ITerrainInfo.clippedTerrainPixels { get { return clippedTerrainPixels; } }
            RectInt ITerrainInfo.clippedPCPixels { get { return clippedPCPixels; } }
            bool ITerrainInfo.gatherEnable { get { return gatherEnable; } set { gatherEnable = value; } }
            bool ITerrainInfo.scatterEnable { get { return scatterEnable; } set { scatterEnable = value; } }
            object ITerrainInfo.userData { get { return userData; } set { userData = value; } }

            public static TerrainTile Make(Terrain terrain, int tileOriginPixelsX, int tileOriginPixelsY, RectInt pixelRect, int targetTextureWidth, int targetTextureHeight)
            {
                var tile = new TerrainTile()
                {
                    terrain = terrain,
                    gatherEnable = true,
                    scatterEnable = true,
                    tileOriginPixels = new Vector2Int(tileOriginPixelsX, tileOriginPixelsY),
                    clippedTerrainPixels = new RectInt()
                    {
                        x = Mathf.Max(0, pixelRect.x - tileOriginPixelsX),
                        y = Mathf.Max(0, pixelRect.y - tileOriginPixelsY),
                        xMax = Mathf.Min(targetTextureWidth, pixelRect.xMax - tileOriginPixelsX),
                        yMax = Mathf.Min(targetTextureHeight, pixelRect.yMax - tileOriginPixelsY)
                    },
                };
                tile.clippedPCPixels = new RectInt(
                    tile.clippedTerrainPixels.x + tile.tileOriginPixels.x - pixelRect.x,
                    tile.clippedTerrainPixels.y + tile.tileOriginPixels.y - pixelRect.y,
                    tile.clippedTerrainPixels.width,
                    tile.clippedTerrainPixels.height);

                if (tile.clippedTerrainPixels.width == 0 || tile.clippedTerrainPixels.height == 0)
                {
                    tile.gatherEnable = false;
                    tile.scatterEnable = false;
                    Debug.LogError("PaintContext.ClipTerrainTiles found 0 content rect");       // we really shouldn't ever have this..
                }

                return tile;
            }
        }
        
    }
}
