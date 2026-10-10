<h1>GENERATED WARZONES</h1>
itch.io link - https://space-tiger-123.itch.io/generated-warzones

---

**Short Description:**
Generated Warzones is a high intensity multiplayer third person shooter game with a battle royale game mode made in a low-poly art style, featuring precision gunplay, dynamic pickups, and procedurally generated island maps for endless variety and unpredictable matches. Customise your character and fight for the most kills!

<img height="350" alt="image" src="https://github.com/user-attachments/assets/78333513-8b7e-4b77-868a-808152d77161" />

**Problems I encountered:**
The terrain was my first experience of procedural generation, and the part I am most proud of. I layered several Perlin noise maps, then converted the values into vertex heights on a 3D mesh. Early on, my persistence was too high, so the island looked like jagged noise, but lowering it and raising lacunarity gave me broad hills with natural-looking detail on top. Noise alone produces a landscape that runs off the edge of the map, so I blended it with a falloff map that lowers the terrain towards the edges, giving a coherent island surrounded by water.

To populate the terrain, my first approach spawned objects at random positions, but during playtesting many ended up inside each other or floating on slopes. I rebuilt it around raycasts, firing down onto the terrain to find the surface point and its normal, so each object sits flush with the ground. Each object type has its own maximum steepness, so buildings only go on flat ground while rocks can sit on slopes. I also cached the positions of placed objects, so each new one could be quickly checked against them and rejected if it overlapped.

Implementing and understanding multiplayer-based systems was a new experience for me. I chose peer-to-peer over dedicated servers for lower latency and no server maintenance, while accepting that the host has authority and that this is less secure against cheating. I used Netcode for GameObjects with Network Variables, Network Lists, and Server and Client RPCs to synchronise animations, terrain and pickups. Bugs often appeared on one side, as something that worked perfectly for the host would break for a client, so I learnt to test both from the start rather than assuming the host’s view was correct. If I rebuilt it, I would design for networking from day one instead of adding it to a single-player foundation.

<img height="350" alt="image" src="https://github.com/user-attachments/assets/56aeae6e-2edb-49a2-8f80-13836ecf371b" />
<img height="350" alt="image" src="https://github.com/user-attachments/assets/3c092113-6619-4816-9f6b-e38d72d31f1b" />
<img height="350" alt="image" src="https://github.com/user-attachments/assets/f3bd4ffb-d408-482d-a3d0-77552ba10b6c" />
