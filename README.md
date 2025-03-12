Debug info about player jumping for DLCQuest game.

Controls:
- Attack/Interact: add a line in console output, useful to visually separate the data
- Jump: Start recording. Stops when no longer pressing Jump and the in-game `jumpTime` stops changing.

Data format:
```
new: 33,3334, old + delta: 33,3334
CanJump: True
Player is jumping
---
```
- `new` and `old + delta` correspond to `jumpTime`, one after the function call, the other before, but adjusted to the change that is usually applied to get current time, represented by delta. In this debug tool it is basically used to show how long the jump has been going, how physics frames look like, and when the jump time gets cleared (which is the only time when these 2 values are different)
- `CanJump` is a function in-game that represents when coyote time is applied. The max length of coyote time is 125 ms, but can be lower a bit due to frame cutoff.
- `Player is jumping` shows up whenever player jumping was detected. When player is not jumping, the line is empty.

Required software for compilation:
- DLC Quest https://store.steampowered.com/app/230050/DLC_Quest/
- BepInEx 6.0.0 https://github.com/BepInEx/BepInEx
  - latest tested version is 6.0.0-be.668 bleeding edge build https://builds.bepinex.dev/projects/bepinex_be
