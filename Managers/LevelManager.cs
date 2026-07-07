using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using SimplePlatformer.Entities;
using SimplePlatformer.Systems;
using LDtk;

namespace SimplePlatformer.Managers;

public static class LevelManager
{
    // Kept alive after load: Draw needs the level's layer data every frame.
    private static LDtkLevel? _level;

    // Tileset textures keyed by the relPath LDtk stores per layer
    // (LayerInstance._TilesetRelPath) — supports any number of tilesets
    // with no hardcoded filenames.
    private static readonly Dictionary<string, Texture2D> _tilesets = new();

    public static Rectangle LevelBounds { get; private set; }
    public static Point PlayerSpawnPoint { get; private set; }

    // Pass-1 clear color authored in LDtk (falls back if no level loaded).
    public static Color BackgroundColor => _level?._BgColor ?? Color.CornflowerBlue;
    
    // TODO: rename to PlayerSpawn on BOTH sides (this class + LDtk editor).
    private class Player : ILDtkEntity
    {
        public string Identifier { get; set; } = string.Empty;
        public Guid Iid { get; set; }
        public int Uid { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Pivot { get; set; }
        public Rectangle Tile { get; set; }
        public Color SmartColor { get; set; }
    }
    public static void LoadContent(SpriteBatch spriteBatch)
    {
        CollisionSystem.Clear();

        LDtkFile file = LDtkFile.FromFile("Assets/SimplePlatformer.ldtk");
        LDtkWorld world = file.LoadSingleWorld();
        _level = world.LoadLevel("Level_0");

        LevelBounds = new Rectangle(_level.Position.X, _level.Position.Y, _level.Size.X, _level.Size.Y);
        BuildCollision(_level);
        LoadTilesets(spriteBatch, _level);
        PlayerSpawnPoint = _level.GetEntity<Player>().Position.ToPoint();
    }

    private static void BuildCollision(LDtkLevel level)
    {
        LDtkIntGrid collisions = level.GetIntGrid("Collisions");
        for (int x = 0; x < collisions.GridSize.X; x++)
        {
            for (int y = 0; y < collisions.GridSize.Y; y++)
            {
                if (collisions.GetValueAt(x, y) == 0) continue;

                var cellPosition = new Point(
                    level.Position.X + x * collisions.TileSize,
                    level.Position.Y + y * collisions.TileSize);
                CollisionSystem.Add(new Solid(cellPosition, collisions.TileSize, collisions.TileSize));
            }
        }
    }

    // Loads every distinct tileset referenced by the level's layers.
    // relPath is relative to the .ldtk file's folder (e.g.
    // "../Assets/Tileset.aseprite"), same resolution ExampleRenderer uses.
    private static void LoadTilesets(SpriteBatch spriteBatch, LDtkLevel level)
    {
        foreach (LayerInstance layer in level.LayerInstances)
        {
            string? relPath = layer._TilesetRelPath;
            if (relPath is null || _tilesets.ContainsKey(relPath)) continue;

            string path = Path.Join(Path.GetDirectoryName(level.WorldFilePath), relPath);

            // File.OpenRead, not TitleContainer: LDtkFile stores WorldFilePath
            // as an absolute path, and TitleContainer only accepts paths
            // relative to the app dir (it exists for console/mobile packaging;
            // plain file IO is fine on desktop).
            using Stream stream = File.OpenRead(path);
            AsepriteFile asepriteFile = AsepriteFileLoader.FromStream(relPath, stream);
            
            Tilemap tilemap = asepriteFile.CreateTilemap(spriteBatch.GraphicsDevice, frameIndex: 0, true);
            
            var target = new RenderTarget2D(spriteBatch.GraphicsDevice,asepriteFile.CanvasWidth, asepriteFile.CanvasHeight);
            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            tilemap.Draw(spriteBatch, Vector2.Zero, Color.White);
            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            _tilesets[relPath] = target;
        }
    }

    // Call inside pass 1's Begin/End in Game1.Draw, BEFORE the player, so the
    // camera matrix + PointClamp apply and tiles render behind actors.
    public static void Draw(SpriteBatch spriteBatch)
    {
        if (_level?.LayerInstances is not { } layers) return;

        // LDtk stores the TOPMOST editor layer first; painter's algorithm
        // wants back-to-front, so iterate in reverse.
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            LayerInstance layer = layers[i];

            if (!layer.Visible || layer._TilesetRelPath is null) continue;
            if (layer._Type == LayerType.Entities) continue;

            // Tiles layers store tiles in GridTiles; AutoLayer and
            // IntGrid-with-rules layers store them in AutoLayerTiles.
            TileInstance[] tiles = layer._Type == LayerType.Tiles
                ? layer.GridTiles
                : layer.AutoLayerTiles;

            Texture2D texture = _tilesets[layer._TilesetRelPath];

            foreach (TileInstance tile in tiles)
            {
                Rectangle sourceRectangle = new Rectangle(tile.Src.X, tile.Src.Y, layer._GridSize, layer._GridSize);
                Vector2 destinationPoint = new Vector2(_level.Position.X + tile.Px.X + layer._PxTotalOffsetX,
                    _level.Position.Y + tile.Px.Y + layer._PxTotalOffsetY);
                
                var effects = SpriteEffects.None;
                if ((tile.F & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
                if ((tile.F & 2) != 0) effects |= SpriteEffects.FlipVertically;
                
                spriteBatch.Draw(texture, destinationPoint, sourceRectangle, Color.White * layer._Opacity, 0f, Vector2.Zero, 1f, effects, 0f);
            }
        }
    }
}
