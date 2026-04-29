# Testing Guide
## How to Check 

---

## Step 1 — Open the Scene
```
Assets → Scenes → SampleScene.unity
```

---

## Step 2 — Press Play ▶

### What you should see immediately:
- Player robot standing at the start of the maze
- AI robot standing next to the player
- Console message: `AIRaceController: NavMesh graph built — 500+ nodes`
- Console message: `AIRaceController: AI waiting 5s before starting...`

---

## Step 3 — Wait 5 Seconds

### What you should see after 5 seconds:
- Console message: `UCS: Path found! Nodes expanded: 700+, Cost: 90+`
- Console message: `AIRaceController: UCS path found — 27 waypoints`
- AI robot starts moving through the maze
- AI robot plays walk/run animation while moving

---

## Step 4 — Test Debug Visualizer

Press **Tab** key during gameplay.

### What you should see:
- ⚪ White spheres — all graph nodes across the maze
- 🟡 Yellow spheres — nodes UCS explored during search
- 🟢 Green spheres and lines — the final path AI is following

Press **Tab** again — all debug visuals disappear.

---

## Step 5 — Test Camera Switch

Press **C** key during gameplay.

### What you should see:
- Camera switches from following the player to following the AI robot
- Console message: `Camera: following AI`

Press **C** again — camera switches back to player.

---

## Step 6 — Wait for AI to Finish

### What you should see:
- AI robot reaches the red goal checkpoint
- Console message: `AIRaceController: AI reached the goal!`
- Console message: `RaceGameManager: AI wins!`
- Screen shows `AI wins!`

---

## Checklist

| Test | Expected Result | Pass/Fail |
|---|---|---|
| Graph builds on start | Console shows 500+ nodes | |
| AI waits before moving | AI stays idle for 5 seconds | |
| UCS finds path | Console shows waypoints and cost | |
| AI moves through maze | Robot walks toward goal | |
| AI animation plays | Robot walks not slides | |
| Tab shows debug nodes | White yellow green spheres visible | |
| Tab hides debug nodes | All spheres disappear | |
| C switches to AI camera | View follows AI robot | |
| C switches back to player | View follows player | |
| AI reaches goal | AI wins message shows | |

---

## Scripts to Check

| Script | Location |
|---|---|
| `UCSSearch.cs` | `Assets/Scripts/AI/` |
| `SearchResult.cs` | `Assets/Scripts/AI/` |
| `DebugVisualizer.cs` | `Assets/Scripts/AI/` |
| `RaceCameraSwitcher.cs` | `Assets/Scripts/AI/` |
| `AIRaceController.cs` | `Assets/Scripts/Race/` |

---

## Controls

| Key | Action |
|---|---|
| WASD | Move player |
| Tab | Toggle debug visualizer |
| C | Switch camera between player and AI |
