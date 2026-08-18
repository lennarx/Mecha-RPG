# Mecha RPG — Project Guide

Tactical grid RPG (Godot 4, .NET/C#), short campaign, anime aesthetic, mecha pilots.
Tone references: *Attack on Titan*, *Evangelion*, *Kaiju No. 8*.

Full narrative bible: `historia_1.md` (Spanish). Full technical/design decisions: `definiciones-tecnicas.md` (Spanish). This file is a condensed, English-language entry point — read the source docs for anything not covered here.

## Goal & scope

- **Primary success criterion: ship a finished game.** Scope is deliberately small so a solo dev or tiny team can complete it.
- Target: 8–10 missions, 4–5 playable chassis, ~20 weapons/modules, 6 enemy types (3 level variants each), 7 bosses ("The Seven").
- Explicitly out of scope: open world/overworld, dialogue-screen story delivery between missions, roaming NPCs, towns.
- **Vertical slice first.** One mission playable end-to-end (grid, movement, one attack, victory condition) before scaling to full scope. If the slice isn't fun, redesign before building more content.
- Platform: PC only (Steam). Grid tactics need mouse precision and screen space; mobile would force F2P/IAP design pressure the project wants to avoid.

## Stack

- Godot 4, **.NET build** (not the standard GDScript-only build) — `dotnet --list-sdks` must return something before you start.
- C#.
- `config/features` in `project.godot` currently targets Godot 4.7, GL Compatibility renderer.

## Non-negotiable coding standards

1. **Weapons, modules and chassis are data, never hardcoded in C#.** Concrete items are `Resource`-derived (`[GlobalClass] partial class ... : Resource`) with `[Export]` fields, authored as `.tres` files.
2. **Effects are composition, never a big `switch`.** A weapon does not "deal damage" — it holds a list of `EffectData` and each concrete effect (`DamageEffect`, and whatever comes next — knockback, overheat, repair...) overrides `Apply(AttackContext)`. Adding a new effect type must never require editing `WeaponData` or adding a switch/if-chain keyed on effect type.
3. **Combat state and logic are pure C#, with zero Godot dependency.** `UnitState`, `AttackContext`, `Combat` (and anything that follows their pattern) must never `using Godot;` or reference Godot types (no `Vector2I`, no `Node`, etc.). This is what makes combat logic testable without opening the editor. Grid/world position, selection state and input belong in the Godot-layer scripts (`Scenes/*.cs`), not in these classes.
4. **Character names always come from `GameConstants.CharacterNames`.** Never a string literal for a character's name in gameplay code. If a name doesn't exist yet, add a placeholder constant there instead of inlining a string.
5. **Tunable balance values live in `GameConstants.CombatConstants`.** No magic numbers for damage, range, HP, move range, multipliers, etc. scattered through the code — add a constant.
6. **All code, comments and identifiers are in English**, even though design docs and team communication are in Spanish.

## Current project state

- `Resources/` — the pure combat layer: `CombatTypes.cs` (enums), `UnitState.cs` (pure C# unit state, includes `MoveRange`), `Combat.cs` (damage-vs-armor math), `EffectData.cs`/`DamageEffect.cs` (composable effects), `WeaponData.cs` (weapon-as-data, `ResolveAttack` entry point), `GameConstants.cs` (all names/constants), `AttackTest.cs` (headless smoke test for the data system), `Resources/Weapons/TrainingBlaster.tres` (a concrete weapon).
- `Scenes/` — the Godot presentation/orchestration layer added for the first vertical slice:
  - `Unit.cs`/`Unit.tscn` — dumb visual wrapper around a combatant (placeholder `ColorRect` + HP `Label`); owns a `UnitState` and its grid `Cell`, nothing else.
  - `Mission.cs`/`Mission.tscn` — the mission orchestrator; the only place that knows about `TileMapLayer`, `AStarGrid2D`, click-to-select/move/attack, and the placeholder victory condition. Builds an 8x8 grid and its `TileSet` procedurally in code (no art assets yet).
  - `GridHighlight.cs` — paints the reachable-move-cell overlay that `Mission.cs` drives.
  - `Mission.tscn` is set as the project's main scene (`project.godot` → `[run] main_scene`).
- This is a **single mission, single playable unit vs. one fixed enemy** ("Training Dummy" — a deliberately non-story placeholder opponent, see `GameConstants.CharacterNames.TrainingDummy`; kept separate from `TheSeven` so a real boss slot isn't spent on a test scene). No enemy AI, no turn order, no second chassis, no configuration UI — that's all future scope once the slice proves out.

## Story summary (see `historia_1.md` for the full document)

A tecnocratic world government (**la Concordia**) keeps peace and order by deciding, via genetic lineage, who is allowed to pilot the giant robots that are its only real military monopoly. The protagonist is a military academy cadet who finds — not inherits — the robot his father hid before dying. High "sync" doesn't make him a better pilot; it makes him detectable to the regime. The central question the game builds toward: does he have the right to break a system that works for most people, in the name of those who can't refuse it? The answer isn't scripted — it's built by the player across a sequence of smaller in-mission moral choices that culminate in a final choice between two costed endings (tear the system down vs. negotiate reform from within).
