# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BattleManager2025 is a **2D turn-based tactical strategy game** built with **Unity 6 (6000.3.6f1)**. It features hexagonal grid-based combat with unit movement, A* pathfinding, and a queue-based combat resolution system. The project also experiments with ML-Agents (PPO) and neural networks (MLP/GRU in Python) for tactical AI.

Primary documentation is in Korean, located in `Assets/Scripts/Documentation/`.

## Build & Run

This is a Unity project — open with Unity Hub or Unity Editor 6000.3.6f1. There is no CLI build pipeline, Makefile, or custom test suite. No `.asmdef` files exist; all scripts compile into the default `Assembly-CSharp`.

Key packages: Unity Input System, Netcode for GameObjects (included but not implemented), UI Toolkit, ML-Agents, Universal Render Pipeline.

## Architecture

### Singleton Managers

Nearly all systems use the singleton pattern (`Instance` property, `DontDestroyOnLoad`). The main singletons and their responsibilities:

- **GameManager** (`Core/`) — Entry point. Owns `UnitManager`, separates units by `playerUnit` flag on startup, drives resource production and turn ticking in `Update()`
- **CombatManager** (`Combat/`) — Orchestrates combat by pairing attackers/defenders and queuing actions via `CombatSequence`
- **TurnManager** (`Core/`) — Manages Action/Strategy turn phases. 1 real second = 1 in-game hour; 24 hours = 1 turn. Uses ScriptableObject events (`GameEventSO`) to broadcast turn changes
- **GridManager** (`GridSystem/`) — Hexagonal grid with dictionary-based tile caching. Provides `GetNeighbors()` for 6-directional hex adjacency (supports pointy-top and flat-top)
- **ResourceManager** (`Core/`) — Economy system with Gold/Food/Wood/Stone, event-driven UI updates
- **CombatEventSystem** (`Combat/`) — Observer pattern with 15+ typed combat events and a timestamped log queue

### Combat Flow

```
GameManager.Start() → finds all Units → separates player/enemy
  → CombatManager.ProcessPlayerAttack()
    → pairs attacker/defender → creates CombatAction
      → CombatSequence.QueueCombatAction()
        → coroutine loop per action:
           1. PrepareForAction (cooldown based on attackSpeed)
           2. Check UnitMode (Defence → wait, Retreat → break)
           3. IsAdjacent? No → FindClosestAdjacentTile → MoveTo
           4. Adjacent → stamina/balance checks → attack/dodge roll → damage calc → death check
```

### Unit System (`Units/Unit.cs`)

The `Unit` class is the core gameplay entity. Key aspects:
- **UnitMode** enum: Normal, Attack, Defence, Retreat — affects behavior in `CombatSequence`
- **Stats**: health, stamina, balance, morale. Stamina/balance recover passively when idle (in `Update()`)
- **Data**: `CharacterData` ScriptableObject holds base stats, class (Infantry/Ranged/Mounted/Special), portrait
- **Movement**: BFS-based `GetMovableTiles()` → `MoveTo()` coroutine with tile occupation tracking
- **Combat**: balance-based dodge formula: `attackSuccessRate = attackerBalance / (attackerBalance + defenderBalance)`

### Damage Calculation (`Combat/DamageCalculator.cs`)

Hybrid system: base damage ± 10% random variation, then defense ratio `100/(100 + defense)`. Defence mode gives 50% reduction. Type advantages: Mounted > Ranged (1.5x), Infantry > Mounted (1.5x), Ranged > Infantry (1.3x).

### Grid & Pathfinding

- **TileCustomWithEvent** extends Unity's `TileBase` — adds A* properties (GCost/HCost/FCost/Parent), unit occupation tracking
- **Pathfinding** (`Core/Pathfinding.cs`) — Standard A* with Manhattan distance heuristic on hex grid
- **GridManager** uses lazy initialization with a `Dictionary<Vector2Int, TileCustomWithEvent>` tile cache

### Event System

ScriptableObject-based (`GameEventSO`): register/unregister listeners for decoupled communication. Used for turn changes (`TurnChangeEvent`, `TurnChangingEvent`) and UI updates (`UIUpdateEvent`).

### ML-Agents / Neural Networks

- `Config/config.yaml` — PPO trainer config for `TacticalCombatAgent` behavior (500K max steps, 64 hidden units)
- `MLP/mlp.py` — PyTorch MLP for tactical decisions (attack/retreat/wait/defend/move)
- `GRU/gru.py` — Sequence-based GRU for temporal combat pattern recognition
- `TacticalCombatAgent~.cs` — Unity ML-Agents agent (note: tilde in filename excludes it from compilation)

## Scenes

- **BattleScene** — Main tactical combat
- **CommandCenterScene** — Base management
- **HubSample2DScene / HubSampleScene** — Hub UI prototypes
- **mlagent_training** — ML-Agents training environment

## Namespaces

- `Core` — GameManager, TurnManager, UnitManager, Pathfinding, ResourceManager, CameraController, GameStateManager
- `Combat` — CombatManager, CombatSequence, DamageCalculator, CombatModifiers, CombatEventSystem, StatusEffectManager, LevelingSystem, FlankingSystem
- `GridSystem` — GridManager, TileCustom, TileCustomWithEvent, Tile
- `Units/` and `Data/` scripts are in the global namespace

## Known Incomplete Areas

- **GameStateManager**: state implementations throw `NotImplementedException`
- **FlankingSystem**: directional attack detection is stubbed
- **Multiplayer**: Netcode packages included but no networking code exists
- **GetClosestTileToTarget** in `Unit.cs` returns null (not implemented)
- Some files use non-standard extensions: `UnitAIController.cs0` in `AI/`
