# SimplePlatformer — Roadmap

A learning-oriented build order for the platformer. Two goals, in priority
order: **(1) build real C#/MonoGame fluency**, and **(2)** end up with a
**clean, reusable 2D-platformer template**.

## How to use this doc

This is a **compass, not a spec**. It sequences the work so you don't paint
yourself into a corner (some things are genuinely painful to retrofit — those
are flagged **⚠️ do early**), but *how* you architect each piece is yours to
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

## Milestone 0 — Foundations & project hygiene ⚠️ do early

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

## Milestone 1 — Get a sprite on screen ⚠️ do early (scaling)

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
  - Content pipeline vs. loading `.aseprite` files directly — when does each
    make sense?

## Milestone 2 — Input you can build on

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

## Milestone 4 — Collision ⚠️ the classic bug factory

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
- **Done when:** an enemy patrols and collides, and player↔enemy contact does
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
