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

**Status:** :emoji_1F504: In progress — `InputHelper.Setup`/`UpdateSetup`/`UpdateCleanup`
are wired into `Game1`'s lifecycle, but that's just Apos.Input's plumbing.
No named actions (`jump`/`left`/`right`) exist yet, and `Game1.Update` still
reads raw `Keyboard`/`GamePad` state directly (currently only for the
Escape-to-quit check). `Managers/InputManager.cs` is still an empty stub —
this milestone's real work hasn't started.

**Goal:** turn raw key/pad state into meaningful, rebindable *actions*.

- **Learn:** **Apos.Input** — `InputHelper` is already wired in `Game1`
  (`Setup`/`UpdateSetup`/`UpdateCleanup`). Learn `Track` conditions and
  virtual buttons/`ICondition`, and the difference between *pressed this
  frame* / *held* / *released* — this distinction matters a lot for jumping.
- **Done when:** "jump", "left", "right" exist as named actions, not scattered
  `Keyboard.GetState()` calls, and you can rebind one without touching gameplay
  code.
- **Decisions:** does `Managers/InputManager` own these action definitions, or
  does Apos.Input's own abstraction already cover it? (Don't wrap a wrapper
  without a reason — figure out what the manager would add.)

## Milestone 3 — A player that moves

**Goal:** position + velocity + gravity, tuned to feel like a platformer.

- **Learn:** integrating motion with `GameTime.ElapsedGameTime` (delta time),
  acceleration/friction/terminal velocity, and why you multiply by `dt`. Basic
  jump as an upward velocity impulse + gravity.
- **Done when:** the player accelerates, has weight, jumps, and falls — and
  *feels* deliberate, not floaty-by-accident.
- **Decisions:**
  - What's the shape of an entity? Fields vs. **properties**, a base
    `Entity` vs. composition — `Entities/Entity.cs` and `Player.cs` are empty
    on purpose. What state does *every* entity share?
  - Units: pixels/second? tiles/second? Pin this down early.
  - **In progress (2026-07-02):** decided to go with an **abstract `Entity`
    base class** (not composition) — discussed lifting `Position` and the
    `Update`/`Draw` method signatures up from `Player`, and weighing
    `abstract` vs `virtual` for `Update`/`Draw` (does shared logic like
    gravity eventually belong in a `virtual` base implementation?). Also
    flagged the Aseprite `LoadContent` pattern (stream → `AsepriteFileLoader`
    → `CreateSprite`) as a candidate for a shared `protected` helper on
    `Entity`, parameterized by file path. User is implementing directly —
    revisit this note once `Entity`/`Player` are actually refactored.
  - **Bug found same day:** `Player.Draw` was passing `position.Y` for both
    X and Y (`new Vector2(position.Y, position.Y)`), and movement math used
    `gameTime.ElapsedGameTime.Seconds` (an `int`, truncates to 0 most frames)
    instead of `.TotalSeconds`/`(float)TotalSeconds` — root cause of "player
    doesn't move." Also noticed `moveUpCondition` uses `.Held()` while the
    other three directions use `.Pressed()` — worth confirming that's
    intentional.
  - **Velocity/max-speed + ms review (2026-07-03):** `Game1` now feeds
    `deltaTime = ElapsedGameTime.TotalMilliseconds`, and `Actor.Update` does
    `MoveX(velocity * dt)` — so **velocity is defined as px/ms**. Four bugs
    identified in `Player.Update`'s input block (user to fix, not Claude):
    1. **Assignment vs accumulation** — lines use `velocity = …` where accel
       needs `velocity += …` (velocity can never build up).
    2. **`* deltaTime` scales the wrong operand** — dt is on the outside
       scaling the whole clamped velocity; it should scale only the
       *acceleration increment* (`velocity ± accel * dt`), with the clamp
       applied *after*, un-scaled.
    3. **Clamp direction inconsistent** — negative dirs (up/left) need
       `Math.Max(-maxSpeed, …)` (floor); positive dirs (down/right) need
       `Math.Min(maxSpeed, …)` (ceiling). Currently moveUp's `Min(maxSpeed…)`
       is a no-op and moveLeft's `Min(-maxSpeed…)` snaps straight to max.
    4. **Friction is frame-rate dependent** — the `MoveTowards(…, maxSpeed/4)`
       decay removes a fixed chunk *per frame*; the max-delta arg needs
       `* deltaTime`.
  - **Units/ms follow-up:** constants `maxSpeed=2000`, `acceleration=1000`
    were tuned for **seconds**; with px/ms they're ~1000× too fast. To stay
    ms-native, scale velocity consts by /1000 (maxSpeed → ~2.0) and accel by
    /1e6 (→ ~0.001). Alternative discussed: keep per-second constants and read
    `TotalSeconds` once instead. **Decision pending** — pick one convention and
    make velocity, acceleration, and friction all agree with it. Also flagged:
    friction runs on the same frame as input accel (decay must be slower than
    accel or max speed is never reached) — confirm that's intended.
  - **Resolved (2026-07-03):** all four bugs above fixed by user — velocity now
    accumulates (`+=`/`-=`), `dt` scales only the accel increment, clamp
    directions correct, friction scaled by `dt` **and** gated behind "no
    direction held" (clean fix for friction-fighting-input). Movement works.
    Current tuning: `maxSpeed=0.5` (px/ms), `acceleration=0.025`. Chose
    ms-native convention.
  - **Remaining polish items (noted, not yet done):**
    1. **Friction is per-input, not per-axis** — the line-47 guard skips
       friction unless *nothing* is held, so holding one axis freezes decay on
       the other (hold Right after tapping Down → Y coasts). Decay X and Y
       independently.
    2. **Diagonal speed ~41% fast** — each axis clamps to `maxSpeed`
       independently (`√(0.5²+0.5²)≈0.707`). Mostly moot once Y becomes
       gravity/jump.
    3. **Accel is near-instant** — reaches `maxSpeed` in ~1–2 frames; widen the
       `maxSpeed`:`acceleration` gap if a ramp/"weight" feel is wanted.
    4. **Still top-down 8-dir** — up/down drive `velocity.Y` directly; replace
       vertical input with gravity + jump impulse per Milestone 3 goal.
    5. Remove the per-frame `Console.WriteLine` debug line.

## Milestone 4 — Collision :warning: the classic bug factory

**Goal:** the player stands on ground and can't walk through walls.

- **Learn:** **AABB** overlap tests (`Rectangle.Intersects`). The standard
  platformer trick: **resolve one axis at a time** (move X → resolve X → move
  Y → resolve Y) to avoid corner-snagging. Discrete vs. swept collision, and
  when tunneling (fast objects passing through thin walls) starts to bite.
- **Done when:** the player lands on solids, is blocked by walls, and you can
  reliably tell whether it's currently *grounded*.
- **Decisions:**
  - Who owns collision — a `Systems/CollisionSystem` that queries the world,
    or does each entity resolve itself against a list of solids? (`Solid.cs`
    and `CollisionSystem.cs` are stubs — this is a real architecture fork.)
  - How is the world queried for nearby solids? (A flat list is *fine* to
    start — don't build a spatial grid before you feel the pain.)
  - One-way platforms now or later?
  - **Decided (2026-07-02):** Actor/Solid split under a shared `Entity` base —
    `Entity` (position/sprite/Draw/Bounds) → `Actor : Entity` (velocity,
    physics, MoveX/MoveY resolution) and `Solid : Entity` (collision geometry,
    no velocity). User restructured into `Core/Entity.cs`, `Actors/`, and
    `Solids/` folders same day.
  - **Decided (2026-07-02):** `CollisionSystem` is **static** (matches the
    static `InputManager`) and is a *queryable database of solids only* —
    `Add`/`Remove`/`Clear` + overlap queries. It does not move entities;
    resolution lives in `Actor.MoveX`/`MoveY` (axis-separated: X fully
    resolves before Y). Trade-offs acknowledged: hidden dependency, one world
    at a time, and `Clear()` must be called on level load or stale solids
    persist. Revisit static-ness at Milestone 12 (template).
  - **Decided (2026-07-02):** positions stay **float** (`Vector2`); simulate
    in floats, snap to ints only at boundaries — `(int)` casts in the computed
    `Bounds` property, rounding at draw time. Int positions would truncate
    sub-pixel per-frame movement (velocity × dt) to zero at low speeds under
    the variable timestep. Sub-pixel remainder accumulation (Celeste-style)
    deliberately deferred until jitter is actually observed.
  - **Jitter observed (2026-07-03):** the deferred sub-pixel issue arrived —
    pushing continuously against a wall makes the *drawn* sprite wobble 1px
    while `Bounds` stays flush. Root cause: pushback is computed in int
    (Rectangle) space but subtracted from the float `position`, so the
    fractional part cycles frame-to-frame and rasterization rounds it
    differently each frame. Plan: (a) refactor `MoveX`/`MoveY` pushback to use
    static `Rectangle.Intersect(a, b)` overlap `Width`/`Height` ×
    `Math.Sign(amount)` instead of manual edge math; (b) zero the sub-pixel
    fraction on the resolved axis when snapping; (c) read the Celeste/
    TowerFall physics article and decide whether full int-position + remainder
    accumulation belongs in the template (Milestone 12 question).
  - **Update (2026-07-03):** flush-snap fix (`Rectangle.Intersect` overlap +
    zero sub-pixel fraction) implemented in `MoveX`/`MoveY` — fixed jitter on
    solids' right/bottom edges but not left/top. Root cause: `(int)` truncation
    in `Bounds` is directionally biased — sub-pixel penetration from the
    right/bottom shifts `Bounds` immediately (detected every frame), while
    from the left/top `Bounds` doesn't move until a full pixel accumulates
    (creep → 1px pop). Aggravated by variable timestep + no v-sync (tiny
    per-frame steps) and gravity re-accelerating from zero while grounded.
    **Decision: adopt Celeste-style movement now** — per-axis float
    `_remainder` banks fractions, position moves in whole pixels only,
    stepping 1px at a time and testing `Bounds` shifted by `Sign(move)`
    before each step (blocked → zero that axis's velocity, break). Deletes
    the snap-out math entirely (never enter a solid), makes draw rounding
    moot, prevents tunneling, and the 1px-shifted query doubles as
    `IsGrounded`.
  - **Progress (2026-07-03):** `_remainder` field added; `MoveX` converted to
    bank → round → early-out → step-loop shape. Remaining: (a) the per-pixel
    blocked check inside `MoveX`'s loop is still a comment; (b) a stray
    pre-loop `FirstOverlap(Bounds)` call in `MoveX` is unused and should go;
    (c) `MoveY` still holds the old snap-out code and — mid-surgery — never
    applies `move` to `position.Y` at all (gravity will stall until it's
    rewritten as the mirror of `MoveX`). Optional refinements discussed:
    `CollidesAt(Point offset)` helper (+ `Shifted` extension method on
    `Rectangle` — note MonoGame's built-in `Rectangle.Offset` mutates in
    place/returns void, so calling it on the computed `Bounds` property
    mutates a throwaway struct copy and does nothing), computed
    `IsGrounded => CollidesAt(0,1)`, and an `Action? onCollide` callback
    instead of hard-coded velocity zeroing.
  - Architecture scaffolding comments added (2026-07-02) to
    `Systems/CollisionSystem.cs`, `Solids/Solid.cs`, `Actors/Actor.cs`, and
    `Core/Entity.cs` — suggested members/signatures live there. Also flagged
    in comments: hitbox fields + `Bounds` belong on `Entity` (not `Solid`);
    hitbox offset should be `Point`/ints not `Vector2`; `Actor.LoadContent`
    duplicates `Entity.LoadContent` (could call `base.LoadContent`);
    `spriteAssetName = null` violates nullable annotations.

## Milestone 5 — Levels & the world

**Goal:** stop hardcoding geometry; load a level from data.

- **Learn:** tilemaps — either MonoGame.Aseprite's tilemap support, or a
  hand-rolled 2D array, or an external editor (**LDtk**/**Tiled**) if
  hand-authoring gets tedious. World-space vs. screen-space coordinates.
- **Done when:** `LevelManager` can load a level into a set of solids/entities
  the rest of the game consumes, and you could add a second level without
  code changes.
- **Decisions:**
  - Author levels in Aseprite, in an editor (LDtk/Tiled), or in code/JSON?
    (Weigh authoring speed vs. how much loader code you want to write/learn.)
  - What *is* a "level" as a data structure? What does loading/unloading own?

## Milestone 6 — Camera

**Goal:** the view follows the player through a level larger than the screen.

- **Learn:** a **view/transform matrix** passed to `SpriteBatch.Begin`, camera
  follow with a **deadzone**, and clamping the camera to level bounds.
- **Done when:** the camera tracks the player smoothly and never shows past the
  edge of the level.
- **Decisions:** roll your own (`Systems/Camera.cs` is stubbed) or evaluate
  **Apos.Camera** first? Either is a fine learning outcome — just make the
  call consciously.

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
other, and the exact folder layout are **yours to design**. The current stubs
(`Entity`/`Player`/`Enemy`/`Solid`, `InputManager`/`LevelManager`,
`Camera`/`CollisionSystem`) are one plausible starting shape — restructure them
the moment a different arrangement fits your mental model better.

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
