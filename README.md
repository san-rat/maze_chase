# Maze Chase 🏃‍♂️

## BSc (Hons) Computer Science — Year 3 Semester 1

### SE3032 Graphics and Visualization | SE3062 Intelligent Systems

### Joint Project — Semester 1, 2026

---

## Project Overview

**Maze Chase** is an interactive 3D racing game where a human player competes against an AI agent to reach the exit of a maze first. The project combines custom 3D environment design with intelligent pathfinding algorithms to create a competitive and visually engaging experience.

The human player navigates the maze manually using keyboard controls, while the AI agent autonomously calculates the optimal path using graph search algorithms and follows it with smooth 3D movement and animations.

---

## Game Concept

* **Genre:** Third Person Racing Game
* **Engine:** Unity 6 (URP)
* **Platform:** PC (Windows / Mac / Linux)
* **Players:** 1 Human vs 1 AI Agent

---

## How to Play

1. Open `Assets/Scenes/SampleScene.unity`
2. Press **Play ▶**
3. Select AI algorithm:

   * Press **1** → UCS
   * Press **2** → BFS
   * Press **3** → A*
4. Race through the maze to reach the red goal checkpoint
5. Beat the AI to win

---

## Team Members & Responsibilities

| Member        | GV Role                                                    | IS Role                     |
| ------------- | ---------------------------------------------------------- | --------------------------- |
| **Student 1** | World Builder — Level Design, Lighting, Texturing, NavMesh | Graph Formulation           |
| **Student 2** | Systems Engineer — Player Interaction Physics              | Dynamic Adaptation & Events |
| **Student 3** | Core Developer — 3D Modeling (Blender     )                | A* Search & Heuristic       |
| **Student 4** | Agent Controller — AI Movement & Animation                 | UCS, BFS & Debug Visualizer |

---

## How to Run

### Requirements

* Unity 6 (6000.4.1f1 or later)
* Universal Render Pipeline (URP)
* Input System
* Cinemachine
* AI Navigation

### Steps

```bash
git clone https://github.com/san-rat/maze_chase.git
```

1. Open Unity Hub → Add Project
2. Select `maze_chase` folder
3. Open `Assets/Scenes/SampleScene.unity`
4. Press **Play ▶**
5. Select algorithm (1 / 2 / 3)

---

## Controls

| Key              | Action                  |
| ---------------- | ----------------------- |
| **WASD**         | Move player             |
| **Mouse**        | Look around             |
| **1**            | UCS                     |
| **2**            | BFS                     |
| **3**            | A*                      |
| **Tab**          | Toggle debug visualizer |
| **C**            | Switch camera           |
| **V**            | Bird’s eye view         |
| **Scroll Wheel** | Zoom                    |

---

## Project Structure

```
maze_chase/
├── Assets/
│   ├── Characters/ — Custom 3D character models
│   ├── Materials/  — Scene materials and textures
│   ├── Minimap/    — Minimap render texture
│   ├── Scenes/
│   │   ├── SampleScene.unity   ← Main game scene
│   │   └── StoneBrickMaze.unity  ← Alternative scene
│   ├── Scripts/
│   │   ├── AI/     — Pathfinding and debug scripts
│   │   │   ├── UCSSearch.cs
│   │   │   ├── BFSSearch.cs
│   │   │   ├── AStarSearch.cs   
│   │   │   ├── SearchResult.cs
│   │   │   ├── DebugVisualizer.cs
│   │   │   ├── RaceCameraSwitcher.cs
│   │   │   ├── AlgorithmSelectorUI.cs
│   │   │   └── AIFootstepHandler.cs
│   │   ├── Race/
│   │   │   ├── AIRaceController.cs
│   │   │   ├── RaceGameManager.cs
│   │   │   ├── RaceParticipant.cs
│   │   │   ├── RaceGoalCheckpoint.cs
│   │   │   └── ThirdPersonCameraBinder.cs
│   │   ├── Editor/
│   │   └── Testing/
│   ├── StarterAssets/   — Unity Starter Assets (Third Person)
│   ├── Textures/        — Stone brick and other textures
│   ├── TextMesh Pro/        
│   ├── TutorialInfo/        
│   ├── Settings/            
│   └── _Recovery/           
│
├── Library/                 
├── Logs/                    
├── ProjectSettings/         
├── UserSettings/            
├── Setup Guide In-Editor Tutorial/ 
└── Packages/
```

---

## AI Pathfinding

### Graph Construction

* Nodes generated from NavMesh (~1400 nodes)
* Nodes connected within radius (6 units)
* Edge cost = Euclidean distance

---

### Algorithms Implemented

#### UCS (Uniform Cost Search)

* Uses priority queue
* Optimal cost path
* Time: O(V log V)

#### BFS (Breadth First Search)

* Uses FIFO queue
* Shortest hops
* Time: O(V + E)

#### A* Search (NEW)

* Uses heuristic + cost
* Faster than UCS
* Optimal with admissible heuristic

---

## Algorithm Comparison

| Property       | UCS            | BFS             | A*             |
| -------------- | -------------- | --------------- | -------------- |
| Data Structure | Priority Queue | Queue           | Priority Queue |
| Uses Cost      | Yes            | No              | Yes            |
| Uses Heuristic | No             | No              | Yes            |
| Optimal        | Yes            | Only unweighted | Yes            |
| Speed          | Medium         | Fast            | Fastest        |

---

## Debug Visualizer

Press **Tab**

| Color     | Meaning        |
| --------- | -------------- |
| 🔵 Blue   | All nodes      |
| 🟡 Yellow | Explored nodes |
| 🟢 Green  | Final path     |

---

## Birds Eye Debug View

1. Press **Tab**
2. Press **V**
3. Scroll to zoom
4. View full search behavior

---

## Scene Setup

### SampleScene

* Maze environment
* Player & AI spawn
* Goal checkpoint
* Minimap
* NavMesh baked

---

## Key GameObjects

| Object               | Purpose           |
| -------------------- | ----------------- |
| PlayerArmature       | Player controller |
| AI_Racer_Robot       | AI agent          |
| RaceGameManager      | Game state        |
| TestDestination_Exit | Goal              |
| GraphNodes           | Pathfinding nodes |
| DebugVisualizer      | Visualization     |
| CameraSwitcher       | Camera control    |

---

## Completed Features ✅

* Full 3D maze environment
* Player movement system
* AI with UCS, BFS, A*
* Algorithm selector UI
* Debug visualization system
* Bird’s eye camera
* Camera switching
* Minimap
* Win condition system
* AI animations
* Full integration across all members

---

## Final Status ✅

* All planned features completed
* A* algorithm integrated
* Project structure finalized
* Ready for submission and demo

---

## Git Commit Convention

```
feat(scope): feature
fix(scope): bug fix
docs(scope): documentation
refactor(scope): cleanup
chore(scope): maintenance
```

---

## Demo Video Checklist

* [ ] Maze overview
* [ ] Algorithm selection
* [ ] UCS run
* [ ] BFS run
* [ ] A* run
* [ ] Debug nodes (Tab)
* [ ] Bird’s eye (V)
* [ ] Camera switch (C)
* [ ] AI reaching goal
* [ ] Player interaction
* [ ] Final comparison

---

## Repository

```
https://github.com/san-rat/maze_chase
```

---
