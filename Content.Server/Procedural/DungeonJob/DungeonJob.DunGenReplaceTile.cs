// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonGenerators;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    /// <summary>
    /// <see cref="ReplaceTileDunGen"/>
    /// </summary>
    private async Task<Dungeon> GenerateTileReplacementDunGen(ReplaceTileDunGen gen, DungeonData data, HashSet<Vector2i> reservedTiles, Random random)
    {
        var tiles = _maps.GetAllTilesEnumerator(_gridUid, _grid);
        var replacements = new List<(Vector2i Index, Tile Tile)>();
        var reserved = new HashSet<Vector2i>();

        while (tiles.MoveNext(out var tileRef))
        {
            var node = tileRef.Value.GridIndices;

            if (reservedTiles.Contains(node))
                continue;

            foreach (var layer in gen.Layers)
            {
                var value = layer.Noise.GetNoise(node.X, node.Y);

                if (value < layer.Threshold)
                    continue;

                Tile tile;

                if (random.Prob(gen.VariantWeight))
                {
                    tile = _tileDefManager.GetVariantTile(_prototype.Index(layer.Tile), random);
                }
                else
                {
                    tile = new Tile(_prototype.Index(layer.Tile).TileId);
                }

                replacements.Add((node, tile));
                reserved.Add(node);
                break;
            }

            await SuspendDungeon();
        }

        _maps.SetTiles(_gridUid, _grid, replacements);
        return new Dungeon(new List<DungeonRoom>()
        {
            new DungeonRoom(reserved, _position, Box2i.Empty, new HashSet<Vector2i>()),
        });
    }
}