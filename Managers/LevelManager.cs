using Microsoft.Xna.Framework;
using SimplePlatformer.Entities;
using SimplePlatformer.Systems;
namespace SimplePlatformer.Managers;
using LDtk;



public static class LevelManager
{
    private static LDtkFile file;
    private static LDtkWorld world;
    private static LDtkLevel level;
    private static LDtkIntGrid collisions;
    public static Rectangle levelBounds {get; private set;}
    public static Point playerSpawnPoint {get; private set;}
    private class Player : ILDtkEntity {
        public string Identifier { get; set; }
        public Guid Iid { get; set; }
        public int Uid { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Pivot { get; set; }
        public Rectangle Tile { get; set; }
        public Color SmartColor { get; set; }
    }

    public static void LoadContent()
    {
        CollisionSystem.Clear();
        file = LDtkFile.FromFile("Assets/SimplePlatformer.ldtk");
        world = file.LoadSingleWorld();
        level = world.LoadLevel("Level_0");
        levelBounds = new Rectangle(level.Position.X, level.Position.Y, level.Size.X, level.Size.Y);
        collisions = level.GetIntGrid("Collisions");
        for (var i = 0; i < collisions.GridSize.X; i++)
        {
            for (var j = 0; j < collisions.GridSize.Y; j++)
            {
                if (collisions.GetValueAt(i, j) != 0)
                {
                    // Create a Solid at the position of the cell
                    Point cellPosition = new Point(level.Position.X + i * collisions.TileSize, level.Position.Y + j * collisions.TileSize);
                    Solid solid = new Solid(cellPosition);
                }
            }
        }
        playerSpawnPoint = level.GetEntity<Player>().Position.ToPoint();
    }

    // TODO(LDtk 6): collision data. Turn the IntGrid into Solids:
    //   var grid = level.GetIntGrid("Collisions");
    // grid.GridSize is the layer size in cells, grid.TileSize the cell size in px.
    // Loop the cells; wherever grid.GetValueAt(x, y) != 0, create a sprite-less Solid
    // (see TODO(LDtk 5) in Solid.cs) at level.Position + cell * TileSize, TileSize²
    // big. Call CollisionSystem.Clear() first so reloading/swapping levels doesn't
    // leak the old geometry. NOTE: one Solid per tile is the O(n) trap flagged in
    // TODO.md Milestone 5 — fine to start, but plan to greedy-merge horizontal runs
    // of solid cells into single wide Solids (or add a grid-lookup query path to
    // CollisionSystem) once this works.

    // TODO(LDtk 7): rendering data, load-time half. Field: LDtkRenderer (namespace
    // LDtk.Renderer), constructed with the game's SpriteBatch. After loading the level
    // call renderer.PrerenderLevel(level) ONCE — it bakes every visual layer
    // (auto-layers included) into RenderTarget2Ds, loading the tileset textures from
    // the paths stored in the .ldtk file. Because it binds render targets, it must run
    // at load time — never mid-Draw between Game1's two passes.

    // TODO(LDtk 8): rendering, draw-time half. Add a Draw(SpriteBatch) method that
    // calls renderer.RenderPrerenderedLevel(level) — it just draws the baked
    // texture(s), so it must be invoked inside pass 1's Begin/End in Game1.Draw
    // (TODO(LDtk 11)) so the camera matrix and PointClamp apply to the level exactly
    // like every other world sprite.

    // TODO(LDtk 9): entity/spawn data. Declare a small class matching the LDtk entity,
    // e.g. `public class PlayerSpawn { public Vector2 Position; }` — then
    // level.GetEntities<PlayerSpawn>() fills it by matching field names (reflection).
    // Expose the spawn position so Game1 can place the player (TODO(LDtk 10)); enemy
    // spawns follow the same pattern later. This keeps the Milestone 5 decision
    // intact: visuals (7-8), collision (6), and spawn data (9) stay separate concerns
    // behind LevelManager.
}
