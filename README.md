# RobotMaster
2D Mega Man clone and level editor in .NET

This project was stopped after Microsoft abandoned support for XNA.  I later converted the projects to MonoGame and cleaned up the project structure.  The game engine is mostly completed, the game just needs content now.  The level editor is functional but still needs work.

Old readme/changelong starts here:
=======================================

Robot Master is 2D action platformer inspired by Mega Man and Ninja Gaiden developed in the XNA framework. It will be packaged as a fully-playable original game along with an editor for creating custom levels and game assets. It is not yet released to the public.


Version History


v0.3.4 (alpha) (04/23/13)

Engine:
  - (New Tile): Slippery tile. Ice physics!
  - Reworked physics engine a bit, allowing the rigid movement of Mega Man to be augmented with more traditional physics.
  - Changed the font, character spacing, and character limit of the debug console.
  - (Bug Fix): When an object collides with multiple tiles, the center-most tile is resolved first, instead of from left to right. This fixed conveyor belts and ice tiles, so that only the center-most tile's movement effect is applied to Mega Man instead of the left-most.
Editor:
  - When saving a level, the default filename is the currently loaded level's filename.
  - Added "slippery tile" support.
  - The "default", "max", and "min" values for enemies' config panels are bound to the respective enemies' classes in the main engine.
  - (Bug Fix): When clicking on the "Level Layout" panel while in "Add Room" mode, the "selected room" would appear elsewhere instead of auto-selecting the newly added room.
  - (Bug Fix): When saving a level, some "enemy" object types would be saved as "pickups", which, when loaded as pickups, cause the editor crash due to not having pickups of that index available. 

v0.3.3 (alpha) (08/31/12)

Engine:
  - Improved the performance of rendering the tilemap so that less memory is used and the framerate is more stable.
  - 
Editor:
  - Placed enemies and objects can now have configurable properties, such as velocity, color, and state. This information is saved along with the object type in the level xml file. Only the enemies ShieldPatrol and FloatingCan are configurable at the moment - custom UserControls need to be created for other objects that need to be configurable.
  - Support for selecting and painting multiple tiles at once. When selecting the tile from the Object Palette, hold the mouse button and drag it across the desired range of tiles. The Floodfill tool does not work properly with this yet.
  - Enhanced the Status Bar, which reports most of the state changes occurring in the editor.

v0.3.2 (alpha) (08/09/12)

Engine:
  - (New Boss): Ninja. Sample boss for testing AI.
    - Boss room has two passable-platforms that Ninja Man can jump on.
    - (Pattern 1) Ninja runs from one end of the screen to the other, jumping after being on the ground for one second.
    - (Pattern 2) Ninja will jump on top of the platform, throw a diagonal fireball, and then jump high off the platform. He will not jump again until reaching the end of the screen.
    - (Both patterns) After turning around, Ninja will throw a shuriken. If Mega Man is close by after a shuriken is thrown, he goes into Pattern 2. If far away, he goes into Pattern 1.
    - (Both patterns) If Ninja is on the ground and Mega Man in front of him in close range, he will slash his sword. Can only happen once per pass.

v0.3.1 (alpha) (08/07/12)

Engine:
  - Pause screen now displays and allows you to switch to available weapons.
  - Pause screen now displays Energy Tanks and Lives.
  - Added item pickups.
  - (New Pickup): Energy Tank, usable from the Pause screen to refill HP to maximum.
  - (New Pickup): One-Up
  - (New Pickup): ECapsuleSmall, ECapsuleLarge, WCapsuleSmall, WCapsuleLarge.
  - New GameplayScreen cmd: "item [id] [x] [y]" spawns an item identified by [id] at the desired x,y position in the current level.
  - Began work on the first real boss.

v0.3.0 (alpha) (07/19/12)

Screenshot
Engine:
  - When standing still, pressing left or right will move MM by 1 pixel, and will begin to walk 6 frames later. Jumping cancels this effect.
  - Redesigned the game's Start Screen.
  - Redesigned the game's Pause Screen.
  - Improved management of game resources to reduce load times.
  - Fade out on death, fade in on level start.
  - (New Weapon): Duck Fist
  - (Bug Fix): Scrolling the screen vertically now moves MM at the correct velocity.
  - (Bug Fix): Fixed some sprite drawing which would appear "jittery" when the camera was moving.
Editor:
  - Greatly improved performance on loading tilemaps.
  - Added the "Level Layout" pane to the main application window, replacing the "Room Browser" pane and "Add Room" window.
    - Rooms can be created, removed, and moved around throughout the level visually via point-and-click.
    - Checkpoints, the spawn point, and all room exits are displayed as icons over each room.
    - Exits can be added or removed from any screen in any room by mousing over the desired screen and pressing the corresponding arrow key. The editor prevents exits from being placed in invalid locations.
    - Selecting a room will display its tilemap for editing in the Room Tilemap Editor pane.
    - When mousing over rooms, information about the rooms' positions, width, and exits are displayed.

v0.2.9 (alpha) (07/11/12)

Engine:
  - New Enemy: "SpikeyRoller". From Mega Man 5 and 9, this enemy rolls along the ground, ceiling, walls, and any tile it comes across. It is invulnerable to everything but Ninja Sword.
  - New Tileset: Metal Man
  - Death by bottomless pit.
Editor:
  - F5 hotkey to test level.

v0.2.8 (alpha) (07/03/12)

Engine:
  - New Weapon: "Coil". Deploy a springboard that Mega Man can launch Mega Man higher than a normal jump. Easier to use than a Rush Coil. Costs 2 weapon energy.
  - New Enemy: "FloatingCan". From Mega Man 2 and 9, this enemy will continuously spawn (up to 3 at a time) and slowly float toward's Mega Man's position.
  - New Game cmd: "fps" to enable/disable FPS display. When in a level, it will also display object and memory information.

v0.2.7 (alpha) (06/28/12)

Engine:
  - Added death capability. HP < 0? Explode, lock controls, restart level after 3 seconds. TODO: fade in/out, death by bottomless pit and spikes.
  - Classic Mega Man "level start" implemented. Added "READY" text that flashes for 3 seconds when the level begins, followed by the "teleport" animation before gaining control.
  - Fixed bug where screen wouldn't scroll upwards properly when climbing a ladder.
Editor:
  - Improved performance of DisplayTilemapGrid to draw rooms. It still needs heavy improvement, though.

v0.2.6 (alpha) (06/26/12)

Engine:
  - Added the beginnings of music playback! Only .mp3 is supported thus far - .nsf would be nice, but would either be extremely difficult or impossible to do with XNA and .NET. Stage tracks will be chosen semi-randomly from a pool of tracks from Mega Man games.
  - Added a small popup window that appears when the next track in the playlist begins.
  - New GameplayScreen cmd: "hp " sets Mega Man's current HP value.
  - New GameplayScreen cmd: "restartlevel" Restarts the level, including all level objects, without unloading resources. Mega Man is placed at the spawn point with full HP without any lives lost.
  - New GameplayScreen cmd: "iddqd" toggles invulnerability, ignoring collisions with enemies.
Editor:
  - Added a "Test Level" option. This launches the game from the editor and immediately loads the current level, which was saved as a temporary file. Closing the game will return to the editor.

v0.2.5 (alpha) (06/20/12)

Engine:
  - Replaced clunky WPF debug window with cooler in-game command console, activated by pressing the '~' key.
    - New Game cmd: "load " begins the desired level and pauses the game.
    - New GameplayScreen cmd: "pos " sets Mega Man's position in the level.
    - New GameplayScreen cmd: "pause" will pause/unpause the game Update loop.
    - New GameplayScreen cmd: "hitbox" will show/hide object hitboxes.
    - New GameplayScreen cmd: "enemy " will spawn an enemy at the given tile location in the current level.
    - New GameplayScreen cmd: "megamouse" will attach Mega Man to the mouse cursor, and is released on left-click :D
  - Refactored some resource management of level objects, fixing some mysterious crashes and discontinuities when repeatedly exiting and entering levels.

v0.2.4 (alpha) (06/08/12)

Engine:
  - New Enemy: "ShieldPatrol", the classic enemy from Mega Man 4. Patrols back and forth and is only vulnerable from the backside. 4HP. Solar Blaze and Ninja Sword pierce the shield, with the Ninja Sword killing instantly.
  - All bullets can "bounce" in certain situations when colliding with an invulnerable object, such as a ShieldPatrol.
  - A weapon icon is displayed above Mega Man's head for 1 second after switching weapons. (Todo: Allow weapon switching during screen/bossdoor transitions)
  - (Bug Fix) Enemies now properly flash when taking damage.
  - (Bug Fix) The debug window should no longer crash due to failing to set ApartmentState.STA on its thread. Update: Nope, still crashes.

v0.2.3 (alpha) (06/05/12)

Engine:
  - New Enemy: "AngleTurret", which fires bullets that explode on contact with the ground or with Mega Man. It can adjust itself to 4 different angles to change the trajectory of its shot to try and hit Mega Man.
  - New Weapon: "Ninja Sword". Operates the same as the basic attack in Ninja Gaiden! Has 56 energy and deals 3 damage to enemies.
  - Changing weapons now cancels out all active bullets.
  - (Bug Fix) Mega Man will now always animate properly when entering a boss door, and will always stop running after passing through it.
Editor:
  - A "Flood Fill" tool is now available for the Tile Brush to fill in multiple adjacent tiles at once.
  - (Source missing for spawn point! TODO disable floodfill when selecting enemy or obstacle tab, show floodfill cursor, and make hotkey "F") Toolbar buttons for Flood Fill and Set Spawn Point now available on the main tilemap grid.
  - Replaced lazy ArrayIndexOutOfBounds exception handlers with proper index checking, greatly improving editor performance.
  - (Source missing!) Enhanced the "Save Level" dialog.
  - (Source missing!) (Bug Fix) Placed obstacles now appear with the proper size in the room viewer.

v0.2.2 (alpha) (05/22/12)

- You can take damage from enemies. Each Enemy type may define its own damage value and collision check routine, including checking against all bullets owned by the Enemy.
   - Mega Man gets knocked back for 40 frames at 0.6px/frame, and remains invulnerable for another 60 frames after.
   - Mega's sprite flickers every 5 frames during this entire period.
   - Taking damage resets Mega's y-velocity and takes him out of "Jumping" and "Climbing" states.
   - TODO (change): Make knockback animation include the "damage flash" bitmap drawn when a boss is hit
- New option: "Damage Numbers", which appear over the heads of enemies RPG-style when they take damage.
- Improved debug rectangle textures on all objects.

v0.2.1 (alpha) (05/14/12)

- New obstacle: "BossDoor", vertical version. MegaMan and Camera do not call Update() while it's animating. Placing one in the editor actually places a second door next to it on the right, so it appears in the next room. Doors lock after being opened once.
- The screen now transitions when Mega Man is 16 pixels from the edge of a room, instead of 0 pixels.
- Finished the pixel shader palette-swap system for Mega Man's weapon colors.

v0.2.0 (alpha) (05/11/12)


- Added simple "Boss" enemy type support.
   - States: Intro, Health, Fight, Death, Inactive
   - In "Health" state, a health bar fills up 1 HP per 4 frames (28 total HP)
   - Bosses have a 20 frame invulnerability period (with animation) after receiving damage
   - All weapons deal 1 damage, except for specifically-defined weakness weapons
- New enemy: TestBoss (An enemy with relatively no AI to test the boss system)
- New enemy: Jump Spider & Ceiling Spider
- Weapon ammunition/energy implemented, displaying an energy meter next to your HP
- Optional HP meter display is availabe for enemies
- Added "Effect" Entity type. Effects are easily created on the fly with Effect.CreateEffect and are automatically managed by the game.
- Added "DeathExplosion" Effect type. When bosses die, they explode!
- Disabled VSync by default, reducing the jitteriness of the display.
- Began work on a simple HLSL shader to be applied to Mega Man's color palette.
- Began work on a simple Audio Manager using XACT.
- Added "Buster" sound.
- Editor: Adding new rooms using the "Add Room" dialog can be done visually.
- (Bug Fix) Editor: You can now add rooms with a negative X and Y coordinate.
- (Bug Fix) Editor: Fixed crashing when trying to load old levels made before certain enemies and objects were available in the engine.

v0.1.8 (alpha) (04/16/12)


- New enemy: Zoomer
- New obstacle: Rising/Falling Platform
- Support for obstacle placement in editor
- Began work on a way to visually place new rooms in a level

v0.1.7 (alpha) (10/12/11)

- Optmized collision detection
- Debug Mode Window Features
   - "Restart Level" button
   - "Pause" button, which halts all updating on the gameplay screen
   - When paused, press backslash to frame-advance
   - "Draw Hitboxes" checkbox, which will display a rectangle around all entities that check for collisions
   - FPS display

v0.1.6 (alpha) (10/05/11)

- (Bug Fix) Collision detection issue has been solved!
- New weapon: Water Shield

v0.1.5 (alpha) (09/26/11)

- Enemies spawn when entering a room only when the screen stops auto-scrolling.
- Enemies in the GameScreen can spawn in customizable locations relative to their tile placement in the editor.
- Editor will ask if you want to save after making changes and trying to close the window, creating a new level, or opening another level.
- Editor has a new toggle button that turns on and off "screen rectangles", which show you the boundaries of each screen in a room.
- New Tile types: Left & Right Conveyors! Impassable tiles that push Mega Man around when standing on them. Has level editor support.
   - TODO (bug): Conveyor collision on the right side occurs earlier than from the left side. Conveyors should only act when Mega Man's exact center is above the conveyor tile.

v0.1.4 (alpha) (09/23/11)

Editor now supports placement of enemies on tilemaps!
   - However, enemies are not customizable. Enemy statistics are hard-coded for now.
   - Selectable "layers" for editing Tiles, Enemies, or Special Objects (WIP).
   - TODO (change): Make enemies spawn in a new room only when it finishes scrolling.
   - TODO (change): Enemies need better positioning from tile placement in the editor to placement in the GameScreen.
   - TODO (change): Enemies need better positioning in the Tilemap display on the Level Editor.

v0.1.3 (alpha) (09/20/11)

Editor Updates
   - Dynamic resizing of the window, which will stretch the tilemap and fix all other UI elements. Min height/width of 1000x600.
   - The right mouse button now erases tiles in the tilemap.
   - Screen number of the current mouse position is now displayed on the statusbar beneath the tilemap
   - (Bug Fix) Tileset dimensions are have been fixed - no more crashing when loading new tilesets.

v0.1.2 (alpha) (09/16/11)



- Enemies now flash when taking damage
- Enemies may now respond to collisions with tiles, similarly to Mega Man (!!!)
- New Enemy: "SmallJumper", which hops across the stage with either tall jumps or short jumps, and collides with the environment
- Added a debug window, activated when in DebugMode

- Bug Fixes
   - Animations are no longer static objects for animated sprites. Multiple sprites now use their own individual animations, fixing the death-event problem encountered yesterday.

v0.1.1 (alpha) (09/15/11)

- Enemies have life, and can now be killed.
- Enemies have death animations.
   - TODO (bug): "death" event fires on the end of the death animation, but it syncs up with all other death animations occuring.
- The current weapon types respond to enemies as they do in MM10. Buster shots always disappear upon collision, Triple Blades only disappear when the shot doesn't kill the enemy, and Solar Blaze shots just plow through everything.
   - TODO (bug): Solar Blaze instant kills everything! Since it's supposed to travel through enemies, flags needs to be set for each individual enemy that is hit such that damage is only dealt once per enemy.
- Support for enemies to fire bullets has been implemented. Some enemy bullets may be destroyed with Mega Man's bullets.
- New enemy: "MedusaHead", which just flys across the screen and shoots destroyable bullets.
   - TODO (feature): Level editor support for enemies

v0.1.0 (alpha) (09/14/11)

- Base game engine
  - Basic menu capabilities, Stage Select screen, Gameplay Screen, Pause Screen.
  - Basic tile collisions. Moving, jumping, shooting, and ladder climbing.
    - TODO (bug): When moving quickly and colliding with the side of a tile, collision is resolved in the incorrect direction. This bug apparently exists in many platformers.
    - TODO (feature): "slow" walking and the associated animation.
  - Mega Buster, Triple Blade, and Solar Blaze weapons are added. Bullet object management for Mega Man and all enemies is in place.
    - TODO (change): Get correct colors for each weapon, hopefully somehow using a palette-swap.
  - Framework for enemies has been laid out - enemies just now need to be made. 

- Editor
   - Tileset importing, with configurable dimensions.
    - Collision mapping for tileset elements
    - Primitive animation editing for tiles, but needs work
   - "Room" and "Exit" configuration and management.
   - Room Grid, which is essentially a canvas for making the level.
     - Spawn point can be set to exactly one Room in a Level
     - Zooming capability and "Show Grid" button for the grid
     - Level Preview, showing all the Rooms in a Level put together
   - Level serialization, compatible with base game engine.
