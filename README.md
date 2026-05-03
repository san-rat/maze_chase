# Maze Chase 🏃‍♂️
## BSc (Hons) Computer Science — Year 3 Semester 1
### SE3032 Graphics and Visualization | SE3062 Intelligent Systems
### Joint Project — Semester 1, 2026

---

## Project Overview

Maze Chase is an interactive 3D race game where a human player competes against an AI agent to reach the exit of a maze first. The project combines custom 3D environment design with intelligent pathfinding algorithms to create a competitive and visually engaging experience.

The human player navigates the maze manually using keyboard controls while the AI agent autonomously calculates the optimal path using graph search algorithms and follows it using smooth 3D character movement and animations.

---

## Game Concept

- **Genre:** Third Person Racing Game
- **Engine:** Unity 6 (URP)
- **Platform:** PC (Windows/Mac/Linux)
- **Players:** 1 Human vs 1 AI Agent

### How to Play
1. Open `Assets/Scenes/SampleScene.unity`
2. Press **Play ▶**
3. Select the AI pathfinding algorithm — press **1** for UCS or **2** for BFS
4. Race through the maze to reach the red goal checkpoint
5. Beat the AI before it reaches the exit

---

## Team Members & Responsibilities

| Member | GV Role | IS Role |
|---|---|---|
| **Student 1** | World Builder — Level Design, Lighting, Texturing, NavMesh | Custom Graph Formulation |
| **Student 2** | Systems Engineer — Player Interaction Physics | Dynamic Adaptation & Event Interception |
| **Student 3** | Core Developer — Custom 3D Modeling (Blender/Maya) | A* Search & Heuristic Design |
| **Student 4** | Agent Controller — AI Animation & Movement | Secondary Search (UCS/BFS) & Debug Visualizer |

---

## How to Run

### Requirements
- Unity 6 (6000.4.1f1 or later)
- Universal Render Pipeline (URP)
- Input System package
- Cinemachine package
- AI Navigation package

### Steps
1. Clone the repository
```
git clone https://github.com/san-rat/maze_chase.git
```
2. Open Unity Hub → Add Project → select `maze_chase` folder
3. Open `Assets/Scenes/SampleScene.unity`
4. Press **Play ▶**
5. Press **1** or **2** to select algorithm and start the race

---

## Controls

| Key | Action |
|---|---|
| **WASD** | Move player through maze |
| **Mouse** | Look around |
| **1** | Select UCS algorithm at game start |
| **2** | Select BFS algorithm at game start |
| **Tab** | Toggle AI debug visualizer on/off |
| **C** | Switch between player and AI camera |
| **V** | Toggle birds eye top down view |
| **Scroll Wheel** | Zoom in/out in birds eye view |

---

## Project Structure

```
maze_chase/
├── Assets/
│   ├── Characters/          — Custom 3D character models
│   ├── Materials/           — Scene materials and textures
│   ├── Minimap/             — Minimap render texture
│   ├── Scenes/
│   │   ├── SampleScene.unity    ← Main game scene
│   │   └── StoneBrickMaze.unity ← Alternative scene
│   ├── Scripts/
│   │   ├── AI/              — Pathfinding and debug scripts
│   │   │   ├── UCSSearch.cs
│   │   │   ├── BFSSearch.cs
│   │   │   ├── SearchResult.cs
│   │   │   ├── DebugVisualizer.cs
│   │   │   ├── RaceCameraSwitcher.cs
│   │   │   ├── AlgorithmSelectorUI.cs
│   │   │   └── AIFootstepHandler.cs
│   │   ├── Race/            — Core race scripts
│   │   │   ├── AIRaceController.cs
│   │   │   ├── RaceGameManager.cs
│   │   │   ├── RaceParticipant.cs
│   │   │   ├── RaceGoalCheckpoint.cs
│   │   │   └── ThirdPersonCameraBinder.cs
│   │   ├── Editor/          — Editor utility scripts
│   │   └── Testing/         — Test scripts
│   ├── StarterAssets/       — Unity Starter Assets (Third Person)
│   └── Textures/            — Stone brick and other textures
└── Packages/
```

---

## AI Pathfinding (Member 4)

### Graph Construction
The AI builds a graph at runtime by sampling the baked NavMesh across the entire maze. Over 1400 nodes are generated covering all walkable corridors. Nodes within 6 units of each other are connected as neighbours with edge cost equal to their Euclidean distance.

### Algorithm Selection
At the start of each game the player selects which algorithm the AI uses by pressing 1 or 2 on the keyboard.

**UCS — Uniform Cost Search**
- Expands nodes in order of cumulative path cost
- Uses a priority queue (min-heap)
- Guarantees the lowest cost path
- Time complexity: O(V log V)

**BFS — Breadth First Search**
- Expands nodes level by level
- Uses a regular queue (FIFO)
- Guarantees fewest hops but not lowest cost
- Time complexity: O(V + E)

### Algorithm Comparison

| Property | UCS | BFS |
|---|---|---|
| Data Structure | Priority Queue | Regular Queue |
| Edge Weights | Considers cost | Ignores cost |
| Path Quality | Optimal lowest cost | Fewest hops |
| Time Complexity | O(V log V) | O(V + E) |
| Optimal | Always | Unweighted only |

### Debug Visualizer
Press **Tab** during gameplay to see the algorithm working in real time:

| Color | Size | Meaning |
|---|---|---|
| 🔵 Blue spheres | Small | All graph nodes — complete search space |
| 🟡 Yellow spheres | Medium | Nodes the algorithm explored during search |
| 🟢 Green spheres | Large | Final optimal path the AI followed |

### Birds Eye Debug View
1. Press **Tab** to enable debug nodes
2. Press **V** to switch to top down camera
3. Use scroll wheel to zoom in and out
4. See the entire maze covered with algorithm visualization from above

---

## Scene Setup

### SampleScene (Main Scene)
- Stone brick maze environment
- Player spawn point and AI spawn point at different locations
- Red goal checkpoint at maze exit
- Minimap camera in top right corner
- NavMesh baked for humanoid agents

### Key GameObjects

| Object | Purpose |
|---|---|
| `PlayerArmature` | Human player character with ThirdPersonController |
| `AI_Racer_Robot` | AI agent with UCS/BFS pathfinding and animations |
| `RaceGameManager` | Tracks race state and declares winner |
| `TestDestination_Exit` | Goal checkpoint with trigger collider |
| `GraphNodes` | Named graph nodes placed across maze |
| `DebugVisualizer` | Renders algorithm visualization in 3D |
| `CameraSwitcher` | Handles C and V camera switching |
| `TopDownCamera` | Orthographic birds eye camera |
| `AlgorithmSelector` | Shows algorithm selection UI at game start |

---

## Branches

| Branch | Member | Purpose |
|---|---|---|
| `main` | All | Stable merged code |
| `feature/member4-ai-debug` | Member 4 | UCS, BFS, debug visualizer |
| `feature/member4-agent-animation` | Member 4 | AI animation, camera, algorithm selector |

---

## Completed Features ✅

- Stone brick maze 3D environment with textures and lighting
- NavMesh baked for full maze coverage
- Player third person controller with walk/run/idle animations
- AI agent with UCS pathfinding — finds optimal cost path
- AI agent with BFS pathfinding — finds shortest hop path
- Algorithm selector UI at game start (press 1 or 2)
- AI walk/run/idle animations driven by NavMeshAgent velocity
- Toggleable debug visualizer showing blue/yellow/green nodes (Tab key)
- Birds eye orthographic camera with scroll zoom (V key)
- Camera switcher between player and AI views (C key)
- Minimap in top right corner showing full maze layout
- Race win condition — first to goal wins
- Consistent Git commit history across all members

---

## Remaining Tasks ⚠️

### Member 2
- Barricade placement and movement physics
- Door opening mechanics
- Object throwing physics

### Member 3
- A* search algorithm with admissible heuristic
- Mathematical justification of heuristic function
- Re-export custom characters with Humanoid rig from Blender for animations

### All Members
- Merge all branches into main
- Test full game flow end to end
- Record 3-minute demo video for GV submission

---

## Submission Requirements

### Graphics and Visualization (SE3032)
- ✅ Source code
- ⬜ 3-minute demo video
- **Deadline: 3rd May 2026**

### Intelligent Systems (SE3062)
- ✅ Git commit history per member
- ⬜ Final showcase and viva
- **Deadline: TBC**

---

## Git Commit Convention

```
feat(scope): description       — new feature
fix(scope): description        — bug fix
docs(scope): description       — documentation
refactor(scope): description   — code cleanup
chore(scope): description      — maintenance
```

---

## Demo Video Checklist

- [ ] Show maze environment overview
- [ ] Show algorithm selector UI at start
- [ ] Select UCS — show AI navigating maze
- [ ] Press Tab — show blue yellow green debug nodes
- [ ] Press V — show full maze from birds eye with nodes
- [ ] Press C — show AI camera following agent
- [ ] Show AI reaching goal — AI wins screen
- [ ] Repeat with BFS — compare path and cost
- [ ] Show custom 3D models
- [ ] Show player barricade interaction
- [ ] Show A* pathfinding comparison
