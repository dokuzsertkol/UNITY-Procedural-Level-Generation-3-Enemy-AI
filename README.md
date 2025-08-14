## GAME CONCEPT
The player is trapped in a dark, labyrinth-like facility and must collect 4 mystical stones to escape.
Each time the player returns to the central room, the entire layout of rooms and hallways changes.
Three different monster types roam the facility, each with unique behaviors.
Gameplay is intensified by limited vision, an ever-changing environment, sudden monster encounters, and a strict time limit.

## GAME LOOP
1. Exit the central room.
2. Collect 4 mystical stones within 120 seconds.
3. Return to the central room.
4. Repeat as long as possible (the level gets procedurally re-generated each iteration).

## CORE MECHANICS
1. Time Limit: 120 seconds per run, resets when entering the central room.
2. Health System: Player starts with 3 HP.
- Monster attacks reduce HP by 1.
- Returning with 4 stones restores 1 HP (max 3).
3. Stones: 4 stones randomly placed in randomly selected rooms.
4 Doors: Central room doors (north/south) only open if 4 stones are collected.
5. Monsters:
- Wanderer: Patrols randomly, chases/attacks if player is detected.
- Sleeper: Sleeps until it hears/sees the player, then chases/attacks.
- Screamer: Roams randomly, screams to alert others when it spots the player.

## PROCEDURAL LEVEL GENERATION 
1. The level generation algorithm takes configurable parameters such as room and hallway size ranges, room count, grid size, collectible count, and prefabs.
2. Grid Creation
- A grid (matrix) is initialized with 0 (empty) and a fixed central room set to 1.
- Rooms are placed randomly within given size limits, avoiding overlaps and out-of-bounds placement.
- Hallways are created by connecting each room to its nearest accessible room, ensuring all rooms can be reached from the central room.
- Disconnected room groups (couple chains) are detected and linked until the level is fully connected.
- Hallway tiles are marked as 3 and expanded to the desired width.
3. 3D Level Building
- Room data is stored in a Room class.
- The grid is scanned to spawn floors, ceilings, and walls using cubes, optimized to cover large continuous areas.
- Hallways follow a “Z” path pattern, and intentional self-intersections add variety to the layout.
4. This approach ensures fully connected, varied, and optimized procedural maps.

## WANDERER AI BEHAVIOUR
1. Wandering: Moves randomly when no player is detected.
2. Chasing: If the player is seen, it chases.
3. Attacking: If the player is close, it attacks.
4. Searching After Lost Sight:
- If the player is not visible for 2s, searches the last known location.
- If neither seen nor heard for 5s after that, returns to wandering.
5. Hearing-Based Detection:
- If the player is heard but not seen, moves to the sound source and searches.
- If spotted during search, switches to chase/attack behavior.

## SLEEPER AI BEHAVIOUR
1. Sleeping: Always starts in a sleeping state.
2. Chasing: Wakes up and chases if the player is seen or heard.
3. Attacking: Attacks if the player is close.
4. Returning to Sleep: If the player is not visible for 5s during a chase, returns to sleeping.

## SCREAMER AI BEHAVIOUR
1. Free Roaming: Moves randomly when no player is detected.
2. Sound Detection: If the player is heard, moves to the last heard location.
3. Screaming: If the player is seen, screams to alert nearby monsters.
4. Reset to Roaming: If the player is neither seen nor heard for 5s, resumes random movement.
- If the player is heard but not seen, moves to the sound source and searches.
- If spotted during search, switches to chase/attack behavior.

## SOME PROCEDURAL GENERATED MAPS
<img width="709" height="705" alt="image" src="https://github.com/user-attachments/assets/82215c09-645d-4ab3-b3a8-c80c500f6795" />
<img width="677" height="607" alt="image" src="https://github.com/user-attachments/assets/015dabba-0f2f-42f8-b9af-b3b762d59557" />
<img width="647" height="694" alt="image" src="https://github.com/user-attachments/assets/2bffec11-7d12-49d3-a13d-4a0997f213ab" />
<img width="1292" height="767" alt="image" src="https://github.com/user-attachments/assets/5a736519-9bcb-4570-90cb-beebf5b421ee" />



## AI FLOWCHARTS
<img width="909" height="592" alt="image" src="https://github.com/user-attachments/assets/c0280f68-2d4b-4ce3-87c0-e930ff726aeb" />
<img width="849" height="531" alt="image" src="https://github.com/user-attachments/assets/dc5274f0-0c86-4231-b1f5-105bc7328324" />
<img width="653" height="628" alt="image" src="https://github.com/user-attachments/assets/9862a6f4-86c2-4945-874f-d2711e0e03c3" />

## GAMEPLAY
- https://youtu.be/Id6bAMbqMu8 First footage of the enemy Sleeper. 
- https://youtu.be/olBtXfVGrto First footage of the enemy Screamer. 
