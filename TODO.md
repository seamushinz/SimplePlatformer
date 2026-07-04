# SimplePlatformer — Roadmap

A learning-oriented build order for the platformer. Two goals, in priority
order: **(1) build real C#/MonoGame fluency**, and **(2)** end up with a
**clean, reusable 2D-platformer template**.

## How to use this doc

This is a **compass, not a spec**. It sequences the work so you don't paint
yourself into a corner (some things are genuinely painful to retrofit — those
are flagged **:warning: do early**), but *how* you architect each piece is yours to
decide. That decision-making is a big part of the learning.

- Each milestone has **Goal**, **Learn** (concepts/APIs to reach for),
  **Done when** (observable outcomes, *not* an implementation), and
  **Decisions** (open questions to answer your own way).
- Reorder freely once you understand the dependencies. The only hard ordering:
  rendering → input → movement → collision. Everything else is negotiable.
- Prefer building the smallest thing that proves a concept, then refactoring.
  A hardcoded gray rectangle that moves is worth more than a perfect Entity
  hierarchy with nothing in it.
- When something feels tedious or repetitive, that's usually the signal to
  stop and extract an abstraction — not before.

---

## Milestone 0 — Foundations & project hygiene :warning: do early

**Status:** :white_check_mark: Done (2026-07-01) — `Nullable`/`ImplicitUsings` on, target
framework confirmed as **net10.0** (csproj), timestep decided (see below).

**Goal:** understand the loop you're building inside of, and lock in a couple
of decisions that are annoying to change later.

- **Learn:** the MonoGame lifecycle — `Initialize` vs `LoadContent` vs
  `Update` vs `Draw`, and *why* they're separate. `GameTime` and fixed vs.
  variable timestep (`IsFixedTimeStep`, `TargetElapsedTime`). What
  `base.Update`/`base.Draw` actually do.
- **Done when:** you can explain, out loud, what runs once vs. every frame,
  and the game window opens and clears to a color you chose.
- **Decisions:**
  - Turn on `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
    now? (Recommended — cheap now, teaches modern C#, painful to retrofit
    once there are warnings everywhere.)
  - Confirm the target framework you actually want (the csproj and any docs
    should agree).
  - Fixed or variable timestep? This affects how you'll write movement/physics
    later, so decide deliberately rather than by default.
  - **Decided (2026-07-01):** variable timestep — `IsFixedTimeStep = false`,
    v-sync also disabled (`SynchronizeWithVerticalRetrace = false`) in
    `Game1`'s constructor. Movement code should integrate with
    `GameTime.ElapsedGameTime` rather than assuming a fixed `dt`.

## Milestone 1 — Get a sprite on screen :warning: do early (scaling)

**Status:** :white_check_mark: Done (2026-07-01) — virtual-resolution scaling and a real
sprite render are both working.

**Goal:** render pixel art correctly, at a resolution you control.

- **Learn:** `SpriteBatch.Begin/Draw/End`, `Texture2D`, and **sampler state**
  — `SamplerState.PointClamp` is what keeps pixel art crisp instead of blurry.
  Loading an Aseprite file with **MonoGame.Aseprite** (`AsepriteFile` →
  `Sprite`/`TextureAtlas`). How the content pipeline (`Content.mgcb`) fits in.
- **Done when:** a sprite you drew in Aseprite renders on screen, unblurred.
- **Decisions:**
  - **Virtual resolution / scaling** (`GameSettings` already has 800×600 but
    nothing uses it): render to a low-res `RenderTarget2D` and scale up, or
    pass a scale matrix to `SpriteBatch.Begin`? Pick one and wire it up now —
    retrofitting a coordinate system after you have gameplay is miserable.
  - **Decided (2026-07-01):** `RenderTarget2D` approach, not a scale matrix.
    `GameSettings.VirtualWidth/Height` (now 400×300, not 800×600 as
    originally noted here) define a low-res target; `Game1.Draw` renders the
    world to it in pass 1, then stretches it into a letterboxed
    `_screenScaleRectangle` on the real window in pass 2, recalculated on
    resize via `OnWindowSizeChanged`/`UpdateScreenScale`. `PointClamp` is
    used on both `SpriteBatch.Begin` calls to keep pixel art crisp.
  - Content pipeline vs. loading `.aseprite` files directly — when does each
    make sense?
  - **Decided (2026-07-01), revised same day:** reversed course — going with
    **direct runtime loading** (`AsepriteFile` via `TitleContainer.OpenStream`
    + `AsepriteFileLoader.FromStream`), not the content pipeline/XNB route.
    Simpler setup (no MGCB Editor wiring, no `nuget.config` workaround needed),
    at the cost of the `.aseprite` file needing `CopyToOutputDirectory` set
    per-file instead of going through `Content.mgcb`. Revisit this trade-off
    again before Milestone 12 (template extraction) if the lack of pipeline
    processing becomes annoying across projects.

## Milestone 2 — Input you can build on

**Status:** :white_check_mark: Done (2026-07-02) — `Managers/InputManager.cs` defines named
`ICondition`s (`jumpOrSelectCondition`, `moveUpCondition`/`Down`/`Left`/`Right`),
each an `AnyCondition` combining keyboard + gamepad. Gameplay code reads these,
not raw `Keyboard`/`GamePad` state.

**Goal:** turn raw key/pad state into meaningful, rebindable *actions*.

- **Learn:** **Apos.Input** — `Track` conditions, virtual buttons/`ICondition`,
  and *pressed this frame* vs. *held* vs. *released*.
- **Decided:** `InputManager` owns the named conditions directly as static
  fields — no extra wrapper layer on top of Apos.Input.

## Milestone 3 — A player that moves

**Status:** :white_check_mark: Done (2026-07-04) — gravity + jump reported working
(alongside Milestone 4's grounded check). Double-check the old cleanup items
below actually got swept up.

**Goal:** position + velocity + gravity, tuned to feel like a platformer.

- **Learn:** integrating motion with delta time, acceleration/friction/terminal
  velocity, and why you multiply by `dt`. Basic jump as an upward velocity
  impulse + gravity.
- **Done when:** the player accelerates, has weight, jumps, and falls.
- **Decided:**
  - Abstract `Entity` base class (not composition) — `Core/Entity.cs` holds
    position/sprite/`Bounds`/`Draw`/`LoadContent`; `Actor : Entity` adds
    velocity and movement.
  - **Velocity is px/ms**, not px/second (`Game1` passes `TotalMilliseconds`
    as `deltaTime`). Current tuning: `maxSpeed = 0.5`, `acceleration = 0.01`.
- **Leftover cleanup to verify:**
  1. ~~Gravity + jump impulse~~ — done per user (2026-07-04).
  2. Diagonal speed clamp issue — likely moot now that Y is gravity-driven;
     confirm.
  3. Remove the per-frame `Console.WriteLine` debug line in `Player.Update`,
     if still present.

## Milestone 4 — Collision :warning: the classic bug factory

**Status:** :white_check_mark: Done (2026-07-04) — resolution algorithm, gravity landing,
and grounded check reported working.

**Goal:** the player stands on ground and can't walk through walls.

- **Learn:** **AABB** overlap tests. The standard platformer trick: **resolve
  one axis at a time** (move X → resolve X → move Y → resolve Y). Discrete vs.
  swept collision and tunneling.
- **Done when:** the player lands on solids, is blocked by walls, and you can
  reliably tell whether it's currently *grounded*.
- **Decided:**
  - `Actor`/`Solid` split under shared `Entity` base, in `Actors/` and
    `Solids/` folders. `CollisionSystem` (`Systems/CollisionSystem.cs`) is a
    **static**, queryable list of solids only (`Add`/`Remove`/`Clear` +
    overlap queries) — it never moves anything itself.
  - **Celeste/TowerFall-style movement**, adopted after float positions caused
    visible 1px jitter against walls: positions are integer `Point`s, with a
    per-axis float `_remainder` on `Actor` banking sub-pixel motion. Each
    `MoveX`/`MoveY` call steps one whole pixel at a time, testing for a
    collision (`CollidesAt`) before each step and zeroing velocity + stopping
    if blocked. This makes tunneling impossible and removes the jitter
    entirely (see [the article this is based on](https://maddymakesgames.com/articles/celeste_and_towerfall_physics/index.html)).
- **Later, when needed:** consider an `Action? onCollide` callback instead of
  hard-coded velocity zeroing in `MoveX`/`MoveY`, if/when something other than
  "stop" is needed (e.g. one-way platforms, hazards).

## Milestone 5 — Levels & the world

**Note (2026-07-04): deliberately reordered — Milestone 6 (camera) is being
done *before* this one.**

**Goal:** stop hardcoding geometry; load a level from data.

- **Learn:** tilemaps, tileset rendering (2D array of tile indices + source
  rects from a tileset texture), world-space vs. screen-space coordinates,
  parsing an external level format.
- **Done when:** `LevelManager` can load a level into a set of solids/entities
  the rest of the game consumes, and you could add a second level without
  code changes.
- **Decided (2026-07-04), from the roguelike-levels discussion:**
  - **End goal:** vampire-survivors-like platformer with randomized levels.
    Chosen generation model is **Spelunky-style chunk stitching** — a library
    of hand-authored room chunks with compatible open edges, assembled
    randomly at runtime — rather than fully procedural generation (which
    requires solving traversability guarantees).
  - **Editor: LDtk** (via `LDtkMonogame` or a hand-rolled JSON parser — the
    format is well documented). Deciding factor: user wants an **autotiling
    brush**, which rules out Aseprite tilemap layers (no autotiling, weak
    entity/metadata placement). LDtk gives auto-tiling rules, IntGrid layers
    (a ready-made collision grid), and entity layers with custom fields.
  - **Ruled out:** Tiled + MonoGame.Extended — large dependency that overlaps
    with hand-rolled learning systems, and its importer wants the content
    pipeline this project bypasses.
  - **Runtime representation** should keep three concerns separate, so the
    generation strategy (single map vs. stitched chunks) is swappable behind
    `LevelManager`: (1) visual tile layers, (2) collision data, (3) entity/
    spawn data.
- **Still open:**
  - Collision representation: per-tile `Solid`s are O(n) death — either
    **greedy-merge** solid tile runs into fewer large `Solid`s (keeps
    `CollisionSystem` untouched), or add a **grid-based query path** to
    `CollisionSystem` (tile-coordinate lookup, O(1); the idiomatic endgame
    for tile platformers, incl. Celeste).
  - `LDtkMonogame` library vs. hand-rolled LDtk JSON parser (learning value
    vs. speed).
  - Chunk edge-compatibility scheme for the stitcher.

## Milestone 6 — Camera :emoji_1F504: current focus (2026-07-04, pulled ahead of Milestone 5)

**Goal:** the view follows the player through a level larger than the screen.

- **Learn:** a **view/transform matrix** passed to `SpriteBatch.Begin`, camera
  follow with a **deadzone**, and clamping the camera to level bounds.
- **Done when:** the camera tracks the player smoothly and never shows past the
  edge of the level.
- **Requirements (2026-07-04):** smooth follow of the player (no harsh
  snapping) and **pixel-perfect** rendering. Approach discussed:
  - Camera = a float `Vector2` position (player-center target) turned into an
    inverse translation `Matrix` passed to `SpriteBatch.Begin(transformMatrix:)`
    on the **world/render-target pass only** (UI pass stays untransformed).
  - Smoothing via **exponential decay** toward the target, made
    framerate-independent (mandatory — variable timestep) with a
    `1 - exp(-k*dt)` style factor, *not* a fixed lerp fraction per frame.
  - Pixel-perfect: keep the float position internally, **round to integers
    only when building the matrix**, matching the integer-`Point`-plus-
    remainder convention actors already use.
  - Deadzone, look-ahead, and level-bounds clamping deferred until levels
    exist (Milestone 5).
- **Decisions:** roll your own (`Systems/Camera.cs` is stubbed) or evaluate
  **Apos.Camera** first? Either is a fine learning outcome — just make the
  call consciously. (Leaning hand-rolled: virtual-resolution scaling — most
  of what Apos.Camera adds — is already built.)

## Milestone 7 — Animation

**Goal:** the player *looks* alive and reflects its state.

- **Learn:** Aseprite animated sprites / `AnimatedSprite` and tags from
  MonoGame.Aseprite. A small **state machine** mapping player state
  (idle/run/jump/fall) to animations.
- **Done when:** animation follows movement state and flips with facing
  direction.
- **Decisions:** where does "current animation state" live — on the player, or
  a separate animation component? (First hint of whether you're leaning toward
  components.)

## Milestone 8 — Game feel

**Goal:** the difference between "it works" and "it's good."

- **Learn:** **coyote time** (jump shortly after leaving a ledge), **jump
  buffering** (press jump just before landing), **variable jump height**
  (release to cut the jump short). Optionally squash/stretch or **Apos.Tweens**
  for juice.
- **Done when:** jumping feels forgiving and responsive; you can name which
  techniques you added and why.
- **Decisions:** which of these are template-worthy defaults vs. per-game
  tuning knobs?

## Milestone 9 — Enemies & interaction

**Goal:** something to actually play against.

- **Learn:** reusing your entity/collision foundations for a second actor;
  simple AI (patrol, edge/wall detection); damage, hazards, and a basic
  health/death loop.
- **Done when:** an enemy patrols and collides, and player:emoji_2194:enemy contact does
  *something*.
- **Decisions:** how much of `Entity`/`Player` genuinely generalizes to
  `Enemy`? This is where an entity base class earns its keep — or reveals it
  was premature.

## Milestone 10 — Scenes & game states

**Goal:** more than one screen (menu → play → pause → game over).

- **Learn:** a scene/screen manager or game **state machine**; separating
  per-scene `Update`/`Draw` from the global loop; transitions.
- **Done when:** you can move between at least two states cleanly, each owning
  its own update/draw.
- **Decisions:** where does this live relative to `Game1`? How much does
  `Game1` shrink once scenes exist?

## Milestone 11 — Audio (light touch)

**Goal:** SFX and music, without over-investing.

- **Learn:** built-in `SoundEffect` (SFX) vs. `Song` (music) and the content
  pipeline for audio. (FMOD is overkill unless you specifically want adaptive
  audio.)
- **Done when:** jump/land SFX and a looping track play.

## Milestone 12 — Extract the template

**Goal:** deliver on goal #2 — make this liftable into the next project.

- **Learn:** what's *game-specific* vs. *reusable infrastructure*, and how to
  separate them (namespaces, folders, or even a shared project/library).
- **Done when:** you can articulate the copy-paste boundary — "these systems
  come along, this content/tuning stays behind" — and the reusable parts don't
  depend on this game's specifics.
- **Decisions:** class library vs. template repo vs. just a well-organized
  folder to clone? What are the seams?

---

## Principles to carry through all of it

- **Update/Draw separation** — logic mutates state in `Update`; `Draw` only
  reads and renders. Don't move things in `Draw`.
- **Make it work → make it right → make it fast**, in that order. Don't
  optimize (spatial partitioning, pooling) until profiling or real pain says
  to.
- **Refactor at the moment of friction**, not preemptively. Duplicate once;
  extract on the second or third repeat, when the right shape is obvious.
- **Prefer clarity over cleverness** while learning C#. Idiomatic beats
  terse.
- **Keep tuning values discoverable** (gravity, jump height, speeds) — ideally
  in one place. You'll adjust them constantly.
- **Commit at each working milestone.** A green, playable checkpoint you can
  return to is worth more than one big commit.

## Deliberately left open

Entity model (inheritance vs. composition/ECS-lite), how systems talk to each
other, and the exact folder layout are **yours to design**. `Entity`/`Actor`/
`Solid`/`CollisionSystem`/`InputManager` are real now, but still just one
plausible shape — `Enemy`, `LevelManager`, and `Camera` are still stubs.
Restructure any of it the moment a different arrangement fits better.

## Possible future project — custom cross-file texture atlas packer

**Not a milestone — optional, post-template, "because it'd be cool" territory.**
Came out of a discussion (2026-07-01) about texture atlases: `MonoGame.Aseprite`
already packs each `.aseprite` file into its own atlas automatically
(`TextureAtlas`/`SpriteSheet`), which is fine for a solo platformer. This is
about going further than necessary, on purpose.

**Goal:** one master atlas spanning *multiple* `.aseprite` source files (and
maybe loose PNGs), auto-generated at build time, while still keeping
`MonoGame.Aseprite`'s direct-`.aseprite`-loading workflow (no manual export
step) and animation tag data intact.

**Why this is nontrivial:** `MonoGame.Aseprite` is a translation layer over the
lower-level `AsepriteDotNet` library, which does the actual file parsing. To
combine multiple source files into one atlas, you'd bypass `MonoGame.Aseprite`'s
per-file packing and instead:

- Use `AsepriteDotNet` directly to read raw frame pixel data + animation tags
  from each `.aseprite` file.
- Feed all frames from all files into your own **bin-packing algorithm**
  (MaxRects or a guillotine/skyline packer are the standard choices) to lay
  them out on one shared texture.
- Rebuild `MonoGame.Aseprite`-equivalent `TextureAtlas`/`SpriteSheet`/
  `AnimationTag` objects yourself, pointing at regions in the combined texture
  instead of per-file textures.
- Likely wire this up as a custom **MGCB content pipeline processor**, so it
  runs at build time like any other content import.

**Alternative/simpler paths, for comparison (not the "weird and optimized"
option, but worth knowing exist):**
- Aseprite's own CLI batch mode (`--batch ... --sheet --data`) can pack
  multiple `.aseprite` files into one PNG + JSON directly — but that means
  giving up `MonoGame.Aseprite`'s direct-load workflow and writing/using a
  different runtime loader.
- [TexturePacker](https://www.codeandweb.com/texturepacker) +
  [TexturePacker-MonoGameLoader](https://github.com/CodeAndWeb/TexturePacker-MonoGameLoader) —
  mature external packer with an official MonoGame content-pipeline loader.
- `MonoGame.Extended.Content.Pipeline` bundles its own texture-packer importer
  alongside its other content types.

**Decisions (when you actually get here):**
- Which packing algorithm, and how much do you care about optimal packing
  vs. simplicity of implementation?
- Do you pack once at build time only, or support incremental repacking as
  assets change during development?
- Does this become a standalone reusable library (fits goal #2 — the
  template), decoupled from this specific game?
- How do you validate it's actually a win? (Profile draw calls/texture swaps
  before and after — don't build this on faith.)
