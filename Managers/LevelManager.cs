namespace SimplePlatformer.Managers;

// TODO(LDtk 3): before writing any code here, author a test level in the LDtk editor
// and save it under Assets/ (see TODO(LDtk 1-2) in the csproj). Layers to create —
// the names matter, the code below looks them up by name:
//   - "Collisions": an IntGrid layer (value 1 = solid). Sole source of collision data.
//   - a visual AutoLayer (or Tiles layer) whose autotile rules read from that IntGrid,
//     so painting collision paints art in one stroke — the autotiling brush is why
//     LDtk was chosen (TODO.md Milestone 5).
//   - "Entities": an Entity layer with at least a "PlayerSpawn" point entity.
public class LevelManager
{
    // TODO(LDtk 4): load file → world → level. `using LDtk;` at the top, then fields:
    //   - LDtkFile:  LDtkFile.FromFile("Assets/<name>.ldtk") — parses the JSON at runtime.
    //   - LDtkWorld: file.LoadSingleWorld() (fine for a single-world project).
    //   - LDtkLevel: world.LoadLevel("Level_0") — the current level, by identifier.
    // Also expose the level's pixel bounds (level.Position and level.Size are both
    // world-space px) as a public Rectangle — the camera needs it for TODO(LDtk 12).

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
