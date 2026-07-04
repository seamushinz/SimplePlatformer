# SimplePlatformer

A 2D platformer built with MonoGame. Two goals drive this project, in order:

1. **Learning C# and game architecture** — the user is a relative beginner in C#
   (though experienced in game dev and programming generally in other languages)
   and is using this project to build real fluency with the language and with
   MonoGame idioms.
2. **A reusable template** for future 2D platformer-style MonoGame projects —
   code here should be structured cleanly enough to lift into a new project.

## How Claude should work on this project — READ THIS FIRST

**Do not write code for the user by default.** This is the single most
important instruction in this file. The user is here to learn, not to get a
finished game handed to them.

- Do **not** write full implementations, methods, or classes unless the user
  explicitly says "write this for me", "just give me the code", or equivalent.
- Default mode: explain the relevant concept, point to the specific MonoGame
  API or C# pattern that applies, ask clarifying/guiding questions, and let
  the user write the code themselves.
- If a code example genuinely helps illustrate a point, keep it to roughly
  2-5 lines — illustrative, not a solution to paste in.
- When the user is debugging, help them reason about *why* something is
  happening (state, order of operations, MonoGame lifecycle, etc.) rather
  than just handing over the fix. Ask what they've already tried before
  proposing anything.
- If the user explicitly asks for a code review, point out issues and
  explain them — don't silently rewrite their code. Suggest changes, don't
  replace wholesale.
- Beyond code generation: actively teach. Surface idiomatic C# (properties
  vs fields, structs vs classes, `readonly`, nullable reference types, LINQ
  where appropriate, etc.) and standard MonoGame/game-loop architecture
  (game loop separation, component/entity patterns, content pipeline usage,
  update/draw separation) as it becomes relevant to what they're building.
- **File-editing scope:** Claude should only ever directly edit `CLAUDE.md`
  and `TODO.md` in this repo. Adding a comment to an existing code file is
  fine when it genuinely helps (a pointer, a breadcrumb, flagging a decision
  point) — but Claude should not otherwise create or edit `.cs`/project
  files. Actual implementation code is the user's to write, per the rule
  above.

## Tech stack

- **.NET 10**, MonoGame DesktopGL (`MonoGame.Framework.DesktopGL` 3.8.x),
  `MonoGame.Content.Builder.Task` for the content pipeline (`.mgcb`).
- **[MonoGame.Aseprite](https://github.com/AristurtleDev/monogame-aseprite)**
  (v6.3.1) — loads Aseprite `.aseprite`/`.ase` files directly for sprites,
  animations, and tilemaps. Currently used via **direct runtime loading**
  (`AsepriteFileLoader.FromStream`), not the content pipeline — see notes
  below and `TODO.md` Milestone 1.
- **[Apos.Input](https://github.com/Apostolique/Apos.Input)** (v2.5.0) —
  input handling abstraction (`InputHelper`, `Track`) layered over
  MonoGame's raw keyboard/mouse/gamepad state; supports input tracking,
  virtual buttons, and rebindable actions.
- `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
  are already set in the csproj.

### Other packages worth considering (suggest, don't add unprompted)

These are common in the MonoGame community and would fit this project's
goals. Mention them when relevant, but let the user decide whether to add
each one:

- **[Apos.Tweens](https://github.com/Apostolique/Apos.Tweens)** — tweening/
  easing, from the same author as Apos.Input; pairs naturally with it.
- **[Apos.Camera](https://github.com/Apostolique/Apos.Camera)** — camera
  helper (the project already has a stub `Systems/Camera.cs`, so worth
  comparing against before deciding whether to roll a custom one).
- **[LDtk](https://ldtk.io/) via `LDtkMonogame` or `SimpleTiled`/`MonoGame.Extended.Tiled`** —
  level/tilemap editors with MonoGame loaders, if hand-coding levels becomes
  tedious. LDtk is the more modern, actively maintained option; Tiled is the
  older/more established one.
- **[MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)** —
  a broad grab-bag (animations, tilemaps, particles, collision, UI, camera,
  entity systems). Large surface area — worth discussing scope before
  pulling in, since it can overlap with hand-rolled systems that are good
  learning exercises.
- **[FMOD](https://www.fmod.com/) or MonoGame's built-in `SoundEffect`/`Song`** —
  fine to stick with built-in audio unless the user wants adaptive audio
  later.
- **FNA** — an alternative to MonoGame's DesktopGL backend if
  cross-platform/performance needs change; not a near-term concern.

## Project structure

```
Core/            Game bootstrap and global settings.
  Game1.cs         Main Game class: Initialize/LoadContent/Update/Draw.
  Entity.cs        Base class: position, sprite, Bounds, Draw/LoadContent.
  GameSettings.cs  Static constants (virtual resolution, etc.).
Actors/          Things that move and collide (Entity subclasses).
  Actor.cs         Velocity + MoveX/MoveY collision resolution.
  Player.cs        Player-specific input/movement.
  Enemy.cs         Stub — not started.
Solids/          Static collision geometry.
  Solid.cs         Registers itself with CollisionSystem.
Managers/
  InputManager.cs  Named Apos.Input actions (jump/move up/down/left/right).
  LevelManager.cs  Stub — not started.
Systems/
  Camera.cs          Stub — not started.
  CollisionSystem.cs Static solids list + overlap queries.
Content/         MonoGame content pipeline project — currently unused
                 (sprites load directly from .aseprite files instead).
Program.cs       Entry point — constructs and runs Game1.
```

`Enemy.cs`, `LevelManager.cs`, and `Camera.cs` are still empty stubs — everything
else above has real implementation. See `TODO.md` for what's next.

## Notes on current state

*(Last updated 2026-07-04 — see `TODO.md` for milestone-by-milestone detail.)*

- **Done:** Milestones 0–4 — foundations, sprite on screen, named input
  actions (Apos.Input), player movement (incl. gravity + jump as of
  2026-07-04), and collision (Celeste-style integer-position-plus-remainder
  step loop in `Actor.MoveX`/`MoveY`, plus grounded check).
- **In progress:** Milestone 6 (camera), deliberately pulled ahead of
  Milestone 5 (levels). Goal: smooth (exponentially damped,
  framerate-independent) player-follow camera that stays pixel-perfect —
  float position internally, rounded to integers when building the
  `SpriteBatch` transform matrix for the world pass.
- **Decided for Milestone 5 (levels):** end goal is a vampire-survivors-like
  platformer with randomized levels via **Spelunky-style stitching of
  hand-authored room chunks** (not full procgen). Editor will be **LDtk** —
  the user specifically wants an autotiling brush, which rules out Aseprite
  tilemap layers; Tiled + MonoGame.Extended ruled out (dependency size +
  content-pipeline requirement). Level loading stays behind `LevelManager`,
  keeping visual tiles / collision / entity-spawn data separate.
- **Key architecture decisions so far:**
  - `Entity` (position/sprite/bounds) → `Actor : Entity` (velocity, move,
    collision) and `Solid : Entity` (static geometry).
  - `CollisionSystem` is a static list of solids with overlap queries only
    — it doesn't move anything; `Actor` resolves its own movement against it.
  - Positions are integer `Point`s with a float sub-pixel remainder banked
    per-axis on `Actor` (fixed earlier jitter from float positions).
  - Velocity is **px/ms** (the `deltaTime` passed into `Update` is
    `TotalMilliseconds`), not px/second.
- Rendering: two-pass draw — world renders to a low-res `RenderTarget2D`
  with `PointClamp`, then stretches into a letterboxed rectangle, recalculated
  on resize. `IsFixedTimeStep = false`, v-sync disabled.
- Sprites load directly from `.aseprite` files at runtime
  (`AsepriteFileLoader`), not through the content pipeline — revisit before
  Milestone 12 (template extraction).
