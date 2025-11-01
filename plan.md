# Turn-Based Strategy Game - Development Plan

## Project Overview

**Course:** Inteligencia Artificial - 4º Curso
**Project Type:** Turn-based strategy game with hierarchical AI system
**Engine:** Unity 6000.0.40f1
**Grid Type:** Hexagonal (10x10)
**Players:** 2 (Human vs Human, Human vs AI, AI vs AI support)

## Game Specifications

### Core Gameplay
- **Win Condition:** Capture enemy base (special cell)
- **Unit Types:** 3 types
  - Infantry: Balanced, prefers forests
  - Cavalry: Fast, prefers plains
  - Artillery: Long range, penalized in mountains
- **Resources:** 1 generic resource type
- **Terrain:** Plains, Forest, Mountain, Water
- **Turn-Based:** Players alternate turns, units refresh each turn

### AI Architecture

The AI operates as a **hierarchical system** with three levels:

```
Strategic Level (Global AI)
       ↓ (Generic Orders)
Tactical Level (Unit AI)
       ↓ (Movement Commands)
Operational Level (Pathfinding)
```

## Implementation Plan

### Phase 1: Core Game Foundation ✅

1. **Hexagonal Grid System**
   - HexCoordinates (axial coordinate system)
   - HexCell (individual cells with terrain, occupancy)
   - HexGrid (grid manager, neighbor finding)
   - HexMesh (hexagon geometry generation)

2. **Terrain System**
   - TerrainType enum (Plains, Forest, Mountain, Water)
   - Movement cost per terrain
   - Defense bonuses
   - Visual color coding

3. **Camera & Input**
   - CameraController (pan, zoom, rotate)
   - InputManager (cell selection, mouse interaction)

### Phase 2: Units & Combat ✅

1. **Unit System**
   - Unit class with stats (HP, Attack, Defense, Movement Points)
   - UnitStats ScriptableObject for configuration
   - 3 unit types with different stats and terrain preferences
   - Visual differentiation by player color

2. **Combat System**
   - Attack/defense calculation
   - Range checking
   - Terrain modifiers in combat
   - Unit death and cleanup

3. **Movement**
   - Click to select unit
   - Click to move (validates movement points)
   - Turn-based movement refresh

### Phase 3: Resources & Production ✅

1. **Resource System**
   - Random resource nodes on map
   - Resource collection by occupying cells
   - Resource display
   - Per-turn resource income

2. **Production System**
   - ProductionBuilding class
   - Base cells for each player
   - Unit production at bases
   - Resource cost deduction

3. **Win Condition**
   - Capture enemy base to win
   - Win detection each turn

### Phase 4: AI - Operational Level (Pathfinding) ✅

1. **Basic A* Pathfinding**
   - A* algorithm for hexagonal grids
   - Manhattan distance heuristic
   - Path construction
   - Considers passable terrain

2. **Tactical Pathfinding**
   - Extended A* with tactical weights
   - **Danger cost:** Avoid cells near enemies
   - **Terrain preference cost:** Prefer good terrain for unit type
   - Multi-factor cost function

### Phase 5: AI - Tactical Level (Decision Making) ✅

1. **Behavior Tree Framework**
   - BTNode (base class)
   - BTSelector (OR logic)
   - BTSequence (AND logic)
   - BTAction (execute function)
   - BTCondition (check boolean)

2. **Unit AI**
   - UnitAI component on each AI unit
   - Interprets strategic orders
   - **Order types:**
     - AttackBase: Move toward and capture enemy base
     - DefendZone: Protect specified area
     - GatherResources: Collect resources
     - Retreat: Fall back to safe zone
   - **Autonomous decisions:**
     - Respond to immediate threats
     - Choose tactical paths
     - Decide when to fight or flee

### Phase 6: AI - Strategic Level (Global Coordinator) ✅

1. **Influence Map System**
   - Track friendly vs enemy influence
   - Influence propagation with distance decay
   - Identify safe zones and danger zones
   - Used for strategic decision making

2. **Tactical Waypoints**
   - Waypoint types: Attack, Defense, Rally, Resource
   - Auto-generate waypoints based on game state
   - Priority system
   - Used to define strategic objectives

3. **Strategic Manager**
   - Global AI brain for AI player
   - Analyzes game state:
     - Unit counts
     - Resource levels
     - Territorial control (influence map)
   - Makes strategic decisions:
     - Assign unit roles (attacker/defender/gatherer)
     - Balance aggression vs defense
     - Resource management
   - **Gives GENERIC orders:**
     - Does NOT say "Unit X move to cell Y"
     - DOES say "Attack enemy base" or "Defend this zone"
   - Units interpret and execute autonomously

### Phase 7: Game Flow & UI ✅

1. **Game Manager**
   - Turn management
   - Player switching
   - Resource tracking
   - Win condition checking
   - AI turn execution

2. **UI System**
   - Turn indicator
   - Resource display
   - Control instructions
   - Simple and functional

## AI Decision Flow

```
┌─────────────────────────────┐
│   Strategic AI Manager      │ (Every AI turn)
│  - Analyze game state       │
│  - Update influence map     │
│  - Update waypoints         │
│  - Assign unit roles        │
│  - Give generic orders      │
└──────────┬──────────────────┘
           │ Order: "AttackBase"
           ↓
┌─────────────────────────────┐
│      Unit AI (Tactical)     │ (Each unit)
│  - Receives strategic order │
│  - Behavior Tree evaluation │
│  - Check local threats      │
│  - Decide specific actions  │
└──────────┬──────────────────┘
           │ Action: "Move to cell (5,7)"
           ↓
┌─────────────────────────────┐
│   Tactical Pathfinding      │
│  - Calculate safe path      │
│  - Avoid danger zones       │
│  - Prefer good terrain      │
│  - Return path              │
└──────────┬──────────────────┘
           │ Path: [(3,4), (4,5), (5,7)]
           ↓
      Execute movement
```

## Key Design Principles (KISS)

### Simplifications
- Only 3 unit types
- 1 resource type
- No fog of war
- Simple combat formula
- No animations (instant actions)
- No unit abilities
- No save/load
- No sound effects
- Rule-based AI (no machine learning)

### What Makes It Non-Trivial
- Hexagonal grid mathematics
- Hierarchical AI with 3 levels
- Influence map propagation
- Behavior tree decision making
- Tactical pathfinding with multiple cost factors
- Strategic coordination of multiple units
- Autonomous unit decision making

## File Structure

```
Assets/Scripts/
├── Core/
│   ├── HexCell.cs              # Individual hex cell
│   ├── HexGrid.cs              # Grid manager
│   ├── HexCoordinates.cs       # Coordinate system
│   ├── HexMesh.cs              # Hexagon geometry
│   ├── TerrainType.cs          # Terrain definitions
│   ├── GameManager.cs          # Main game controller
│   ├── CameraController.cs     # Camera movement
│   └── InputManager.cs         # Input handling
├── Units/
│   ├── Unit.cs                 # Unit behavior
│   ├── UnitType.cs             # Unit type enum
│   └── UnitStats.cs            # Unit configuration
├── Combat/
│   └── CombatSystem.cs         # Combat calculations
├── Resources/
│   ├── ResourceNode.cs         # Resource points
│   └── ResourceManager.cs      # Resource tracking
├── Production/
│   └── ProductionBuilding.cs   # Unit production
├── AI/
│   ├── Pathfinding/
│   │   ├── AStarPathfinding.cs         # Basic A*
│   │   └── TacticalPathfinding.cs      # A* with tactical weights
│   ├── BehaviorTree/
│   │   ├── BTNode.cs                   # Base node
│   │   ├── BTSelector.cs               # OR node
│   │   ├── BTSequence.cs               # AND node
│   │   ├── BTAction.cs                 # Action node
│   │   └── BTCondition.cs              # Condition node
│   ├── Tactical/
│   │   ├── UnitAI.cs                   # Unit AI brain
│   │   └── UnitBehaviorTree.cs         # Unit behavior logic
│   └── Strategic/
│       ├── InfluenceMap.cs             # Territorial analysis
│       ├── TacticalWaypoints.cs        # Strategic objectives
│       └── StrategicManager.cs         # Global AI coordinator
└── UI/
    ├── UIManager.cs             # UI controller
    ├── UnitInfoPanel.cs         # Unit information
    └── ResourceDisplay.cs       # Resource display
```

## Implementation Status

✅ **Completed:**
- [x] Hexagonal grid system
- [x] Terrain types and rendering
- [x] Camera and input controls
- [x] Unit system (3 types)
- [x] Player-controlled movement
- [x] Combat system
- [x] Resource system
- [x] Production system
- [x] Win condition
- [x] A* pathfinding
- [x] Tactical pathfinding
- [x] Behavior Tree framework
- [x] Unit AI with order execution
- [x] Influence Map system
- [x] Tactical Waypoints
- [x] Strategic Manager
- [x] AI hierarchy integration
- [x] Basic UI

## How to Use in Unity

1. **Setup Scene:**
   - Create empty GameObject → Add HexGrid component
   - Create Camera → Add CameraController component
   - Create Canvas → Add UIManager component
   - Create empty GameObject → Add GameManager component
   - Create empty GameObject → Add StrategicManager component (set Player ID to 1 for AI)

2. **Create Unit Stats:**
   - Right-click in Project → Create → Game → Unit Stats
   - Create 3 ScriptableObjects: Infantry, Cavalry, Artillery
   - Configure stats for each

3. **Assign References:**
   - Link HexGrid, GameManager in InputManager
   - Link GameManager in UIManager
   - Assign Unit Stats in GameManager

4. **Play:**
   - Press Play
   - Click units to select
   - Click cells to move or attack
   - Press Space to end turn
   - AI will execute automatically on its turn

## Testing Checklist

- [ ] Grid generates correctly
- [ ] Units can move within movement range
- [ ] Combat works with correct damage calculation
- [ ] Resources are collected
- [ ] Units can be produced at bases
- [ ] Win condition triggers when base is captured
- [ ] AI units receive and execute orders
- [ ] Pathfinding avoids obstacles and danger
- [ ] Strategic AI coordinates multiple units
- [ ] Turn system works correctly

## Future Enhancements (Out of Scope)

- More unit types
- Multiple resource types
- Fog of war
- Unit abilities and special powers
- Multiplayer networking
- Advanced AI (Monte Carlo Tree Search, Neural Networks)
- Save/load system
- Animations and effects
- Sound and music

## References

- "Artificial Intelligence for Games" - Ian Millington & John Funge
  - Chapter 5: Decision Making
  - Section 6.1: Waypoint Tactics
  - Section 6.2.2: Simple Influence Maps
  - Section 6.3: Tactical Pathfinding
- https://www.redblobgames.com/pathfinding/a-star/introduction.html
- https://www.redblobgames.com/grids/hexagons/

## Git Commits

1. Initial commit: Unity project setup
2. Core game systems: Grid, terrain, units, combat
3. AI hierarchy: Strategic-Tactical-Operational levels
