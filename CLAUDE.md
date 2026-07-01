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

## Tech stack

- **.NET 10**, MonoGame DesktopGL (`MonoGame.Framework.DesktopGL` 3.8.x),
  `MonoGame.Content.Builder.Task` for the content pipeline (`.mgcb`).
- **[MonoGame.Aseprite](https://github.com/AristurtleDev/monogame-aseprite)**
  (v6.3.1) — loads Aseprite `.aseprite`/`.ase` files directly for sprites,
  animations, and tilemaps via the content pipeline.
- **[Apos.Input](https://github.com/Apostolique/Apos.Input)** (v2.5.0) —
  input handling abstraction (`InputHelper`, `Track`) layered over
  MonoGame's raw keyboard/mouse/gamepad state; supports input tracking,
  virtual buttons, and rebindable actions.
- C# nullable/implicit usings are not yet configured in the csproj — worth
  discussing with the user if it comes up (`<Nullable>enable</Nullable>`,
  `<ImplicitUsings>enable</ImplicitUsings>` are common modern-C# defaults).

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
  GameSettings.cs  Static constants (virtual resolution, etc.).
Entities/        Game objects that live in the world.
  Entity.cs        Base entity (stub).
  Player.cs        Player entity (stub).
  Enemy.cs         Enemy entity (stub).
  Solid.cs         Static/solid collision geometry (stub).
Managers/        Longer-lived subsystems that manage state across frames.
  InputManager.cs  Input handling wrapper (stub) — will likely sit atop Apos.Input.
  LevelManager.cs  Level loading/transition management (stub).
Systems/         Cross-cutting logic that operates over entities/state.
  Camera.cs          Camera/viewport logic (stub).
  CollisionSystem.cs Collision detection/resolution (stub).
Content/         MonoGame content pipeline project (Content.mgcb) — sprites,
                 fonts, levels, etc. get built through here or loaded
                 directly via MonoGame.Aseprite.
Program.cs       Entry point — constructs and runs Game1.
```

Everything under `Entities/`, `Managers/`, and `Systems/` is currently a
stub (empty class body) — the architecture (what goes where, how these
layers talk to each other) is intentionally undecided and is a good subject
for discussion with the user rather than something to fill in unilaterally.

## Notes on current state

- `Game1.cs` wires up `InputHelper.Setup`/`UpdateSetup`/`UpdateCleanup` from
  Apos.Input, and has the standard MonoGame-template `TODO` comments in
  `Initialize`, `LoadContent`, `Update`, `Draw` — this is template
  boilerplate, not yet real game code.
- `GameSettings` defines a virtual resolution (800x600) but nothing yet
  reads or uses it (e.g. no scaling matrix/render target set up).
- No content has been added to `Content/Content.mgcb` yet beyond the
  default project scaffold.
