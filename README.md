# Regression Valley
Regression Valley is a SDV Mod that adds useable diapers, ABDL themed NPC dialgoue, Hunger and Thirst bars and much more!

## Authorship and Attributions
The Original Author is u/Primeval_Titmouse, who has long since deleted his reddit account and has left no other identifying or contact details.

### Thank you to the identified previous maintainers
- Fox Tale Times User: oniryuuko (now contributing on github under the same name)
- Fox Tale Times User: SabrinaM
- Fox Tale Times User: Jon

### Thanks to contributors:
- u/BabyKunoichi
- u/abdlnikki
- u/FurryDestiny
- u/Proximitron

### Thanks to alpha testers:
- u/zombiekarasu
- u/dr_daddy
- u/errycupid
- u/rosebush413
- u/mrnogee
- u/crinklycuddles
- u/vulpix77

## Installation Instructions
### Prerequisites
Follow instructions provided by the individual prerequisite mods and programs.
- Stardew Valley (version 1.6.15)
- [SMAPI](https://smapi.io/) (version 4.2.2)
- [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915?tab=files) (version 2.6.1)
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098?tab=files) Highly recommended: (Allowes ingame configuration of the mod)

### Installation
1) Download the [latest release](https://github.com/X3n0nX/Regression_Valley/releases). Be careful that you don't download the source code. The release is the one that contains Regression.dll.
2) Unzip contents of the release into the Mods directory created during SMAPI installation.
> This should produce a folder "Regression Mod" which should contain two other folders (among other things), "Regression" and "Regression Dialogue".
- Stardew Valley
  - Mods
    - Regression
      - Core
      - Regression Dialogue  

## Controls 
(All key bindings can be changed with the exception of "Shift" and "Alt" Keys)

### Typical
- F1: Attempt to Pee in Pants
- F2: Attempt to Poop Pants
- F4: (Reserved by Game for Screenshot Mode)
- F5: Check State of Underwear
- F6: Check State of Pants
- F7: Check State of Potty Training
- F8: Check Potty Feeling
- F9: Activate / Deactivate Debug Mode
- Left Shift + F1: Pull down pants and attempt to pee.
- Left Shift + F2: Pull down pants and attempt to poop.
- Left Shift + Left Click: Drink when near water source or holding watering can.
- Note, the following locations have toilets (use you imagination, there is no icon/sprite). Pulling down your pants anywhere else will result in you going on the floor (*gross*).
  - House
  - Island House
  - Bathhouse Locker-room

### In Debug
- Right Alt + F1: Make more thirsty and hungry
- Right Alt + F2: Make less thirsty and hungry. Make Bladder and Bowels fuller.
- Right Alt + F3: Open a menu with all valid underwear types. ( + some wine ;) )
- Right Alt + F4: (Reserved by Windows to force quit)
- Right Alt + F5: Fast forward time a bit (do not hold down. Wait for fast forward to finish before pressing again.)
- Right Alt + F6: Toggle Wetting Option
- Right Alt + F7: Toggle Messing Option
- Right Alt + F8: Toggle Easy Mode Option
- Right Alt + W: Increase Bladder Continence
- Right Alt + S: Decrease Bladder Incontinence
- Right Alt + E: Increase Bowel Continence
- Right Alt + D: Decrease Bladder Incontinence

### Change Controls
You have two options to change the key bindings.
1.  Highly recommended: With the Mod "Generic Mod Config Menu".
2.  If you dont have installed the "Generic Mod Config Menu", go to the config.json under the "core" directory of the mod befor you launch the game.

## Basic Mechanics
You now get hungry and thirsty over time, and from doing work. Getting too hungry or thirsty gives you de-buffs. Of course, you can alleviate your hunger by eating food, and slake your thirst by drinking beverages (including water from a water source, your watering can, or things like wine, coffee, etc.).

Just remember, what goes in will eventually come out. You now also have to worry about managing your more private needs. The fullness of your bladder and bowels increase naturally over time, given that you're neither starving nor dying of thirst. Be careful not to over eat or over drink though, it'll quickly pass through you! Currently the only toilet is in your house, so if you're out and about, you have a few options:
- Try to hold it and run home (or another toilet) to use the potty like a big boy/girl.
- Pull down your pants and go right where you are. But be warned! If anyone can see/smell you, they may not quite appreciate your 'performance'!
- Go in your pants! Of course, if you want to do that, you may want to consider some protection, available at Pierre's Seed Shop, who is definitely not our sponsor!
  - If you rely too much on your protection, though, you may just find that it becomes a little bit harder to stop yourself. If you even notice, that is.

## Issues and Ideas
Please feel free to ask questions, report bugs, and request features on the GitHub page.

### Issue Report Guidance
Please include the following in the issue report:

#### Issue Description
Try to be as specific as possible. 
- What were you trying to do?
- What did you expect to happen versus what actually happened?
- Where were you and when?

#### SMAPI Log
##### Enable Content Patcher's verbose logging
1. Open SMAPI's smapi-internal/StardewModdingAPI.config.json in a text editor
2. Change `"VerboseLogging": false` to `"VerboseLogging": true`
3. Save the file.

##### Obtain the log
1. Run the game until you encounter the issue.
2. Exit the game.
3. Press the Windows and R buttons at the same time.
4. In the 'run' box that appears, enter this exact text:
5. %appdata%\StardewValley\ErrorLogs
6. The log file is SMAPI-crash.txt if it exists, otherwise SMAPI-latest.txt.

#### Save file
For most bugs, one of the biggest aids is the Save File, as it would help in debugging the reported issue by more easily recreating the prerequisite conditions.
