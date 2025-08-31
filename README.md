# Board Numbers: Sevens, Elevens, and Ones

A Unity 3D implementation of the abstract board game "Sevens, Elevens, and Ones" built in Unity 2022.3.54f1 (LTS).

## Game Overview

**Sevens, Elevens, and Ones** is a strategic tile-placement game for 2-4 players featuring pattern recognition and scoring bonuses.

### Game Rules

- **Setup**: Each player draws 3 tiles from a deck. Randomize first player.
- **Turn**: Place 1 tile on any empty cell of the 7×7 board
- **Scoring Patterns**:
  - **Seven**: Connected group summing to 7 → +3 points + draw 1 tile
  - **Eleven**: Connected group summing to 11 → +5 points + extra turn  
  - **Triple Ones**: Exactly three connected 1s → +7 points + extra turn
- **Connected Groups**: 3+ tiles with 8-way adjacency (orthogonal + diagonal), no gaps
- **Win Condition**: First to reach 77 points OR highest score when board is full

## Project Structure

```
Assets/
├── Scripts/           # Core game logic (namespace: BoardNumbers)
│   ├── GameManager.cs           # Main state machine and game orchestration
│   ├── PlayerController.cs     # Player data management (POCO)
│   ├── DeckService.cs          # Deck building, shuffling, and drawing
│   ├── BoardGrid.cs            # 7×7 grid management and tile placement
│   ├── PatternRecognitionService.cs  # Scoring pattern detection
│   ├── TileSetConfig.cs        # ScriptableObject for deck configuration
│   ├── GameRulesConfig.cs      # ScriptableObject for game rules
│   ├── GameUI.cs               # Main game UI controller
│   ├── MainMenuController.cs   # Menu system controller
│   └── GameTestRunner.cs       # Test runner for game logic validation
├── ScriptableObjects/ # Game configuration assets
│   ├── DefaultTileSetConfig.asset    # Default deck distribution
│   └── DefaultGameRulesConfig.asset  # Default game rules
├── Scenes/            # Unity scenes
│   ├── MainMenu.unity # Main menu scene
│   └── Game.unity     # Main gameplay scene
├── Prefabs/           # Reusable GameObject prefabs
├── Materials/         # Visual materials
├── UI/               # UI prefabs and assets
├── Audio/            # Sound effects and music
└── VFX/              # Visual effects
```

## Core Features

### ✅ Implemented
- **Complete Game Logic**: Full implementation of game rules and scoring
- **State Machine**: Robust GameManager with Setup → PlayerTurn → ResolveScore → ExtraTurn → GameEnd states
- **Pattern Recognition**: Advanced flood-fill algorithm for detecting connected groups
- **Configurable Rules**: ScriptableObject-based configuration system
- **Pass-and-Play**: Local multiplayer support for 2-4 players
- **Clean Architecture**: Production-ready C# code with proper separation of concerns
- **Mobile-Friendly**: Input system designed for both desktop and mobile
- **Testing Framework**: Comprehensive test runner for game logic validation

### 🎯 Technical Highlights
- **Namespace**: All code organized under `BoardNumbers`
- **POCO Design**: PlayerController as Plain Old C# Object for easy serialization
- **Service Architecture**: Separate services for Deck, Board, and Pattern Recognition
- **Event-Driven UI**: Decoupled UI system with event subscriptions
- **Configurable**: Easy to modify game rules and tile distributions via ScriptableObjects

## Getting Started

### Requirements
- Unity 2022.3.54f1 (LTS) or compatible version
- Standard Render Pipeline (URP not required)

### Setup
1. Open the project in Unity 2022.3.54f1
2. Ensure all scripts compile successfully
3. Configure the `DefaultTileSetConfig` and `DefaultGameRulesConfig` assets as needed
4. Build and run to play

### Default Configuration
- **Tile Distribution**: 1×8, values 2-6×5 each, values 7-11×4 each (59 total tiles)
- **Scoring**: Seven=3pts, Eleven=5pts, TripleOne=7pts
- **Target Score**: 77 points
- **Board Size**: 7×7 grid
- **Hand Size**: 3 tiles per player

## Architecture Details

### State Machine Flow
```
Setup → PlayerTurn → ResolveScore → [ExtraTurn] → [GameEnd]
  ↑                     ↓
  └─── Next Player ←────┘
```

### Pattern Recognition Algorithm
1. **Flood Fill**: Find all connected components including placed tile
2. **Group Validation**: Check groups of 3+ tiles
3. **Pattern Matching**: Test for Seven/Eleven sums and Triple Ones
4. **Bonus Processing**: Apply in order: Triple One → Eleven → Seven

### Data Flow
- `TileSetConfig` → `DeckService` → Shuffled deck
- `GameRulesConfig` → `GameManager` → Game rules
- `BoardGrid` + `PatternRecognitionService` → Scoring
- Events → `GameUI` → Visual updates

## Testing

Run tests via the `GameTestRunner` component:
- Board placement and validation
- Deck shuffling and drawing
- Player hand management
- Pattern recognition accuracy
- Basic game flow validation

## Customization

### Modify Tile Distribution
Edit `DefaultTileSetConfig.asset` to change tile quantities and values.

### Adjust Game Rules
Edit `DefaultGameRulesConfig.asset` to modify scoring, board size, hand size, etc.

### Extend Patterns
Add new scoring patterns by extending `PatternRecognitionService.cs`.

## Future Enhancements
- 3D visual board representation
- Animated tile placement
- Sound effects and music
- AI opponents
- Online multiplayer
- Tournament mode
- Statistics tracking

## License
This project is created as a Unity game development prototype. All code follows Unity C# conventions and best practices for production-ready game architecture.