Debug info about player jumping for DLCQuest game.

Controls:
- Attack/Interact: add a line in console output, useful to visually separate the data
- Jump: Start recording. Stops when no longer pressing Jump and the player character lands on the ground.

Data format:
```
new: 16,6667, old + delta: 16,6667, OffGroundTime: 33,33334
CanJump: True, IsAtCeiling: False
Y delta: -4,985001, Y velocity: -19,96279
jump Y velocity      : -20,86279
jump Y velocity delta: -21,76279, substep gravity: 0,9000018
player Y velocity delta: -20,86279
Player is jumping
---
```
- `new` and `old + delta` correspond to `jumpTime`, one after the function call, the other before, but adjusted to the change that is usually applied to get current time, represented by delta. In this debug tool it is basically used to show how long the current jump has been going, how physics frames look like, and when the jump time gets cleared or stays at 0 (which is the only time when these 2 values are different).
- `CanJump` is a function in-game that represents when coyote time is applied. The max length of coyote time is 125 ms, but can be lower a bit due to frame cutoff.
- `OffGroundTime` and `IsAtCeiling` should be self explanatory.
- `Y delta` is change in Y position of player compared to the start of the jump sequence, `Y velocity` is Y velocity of player.
- `jump Y velocity` is the velocity that is being applied during a jump, `jump Y velocity delta` is a change in that velocity compared to previous frame, `substep gravity` is how much gravity is applied in current frame (doesn't change outside of slowed time).
- `player Y velocity delta` is change in player Y velocity compared to previous frame.
- `Player is jumping` shows up whenever player pressing Jump button was detected. When the button is not pressed, the line is empty.

Required software for compilation:
- DLC Quest https://store.steampowered.com/app/230050/DLC_Quest/
- BepInEx 6.0.0 https://github.com/BepInEx/BepInEx
  - latest tested version is 6.0.0-be.668 bleeding edge build https://builds.bepinex.dev/projects/bepinex_be
- Microsoft XNA Framework Redistributable 4.0 from https://www.microsoft.com/en-us/download/details.aspx?id=20914
