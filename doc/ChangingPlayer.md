

## Getting Changed by Npc

### How it works

### Option 1 (to use in dialoge system directly)
* Change can be triggered directly by defined dialoges, whitch can be find under `Regression Dialogue\Dialogue\NPCs\` by action Placeholder `DIAPER_CHANGE`
* These option needs at least two parameters 
  * `\"npc name\"`        -> name of the npc whitch will give you a change
  * `\"underwear name\"`  -> name of the new underwear, the npc will change us
  * `\"pants name\"`      -> (optional) name of pants, the npc will change us if our pants are messy
* The possible options for `\"underwear name\"` and `\"pants name\"` can be found under `Regression\Data\TypesData.json`
* Example: `#$action DIAPER_CHANGE \"jodi\" \"baby print diaper\" \"sams old pants\"#`
  * We will receive a underwear change from `jodi`
  * She will change us into a `baby print diaper`
  * If our pants are messy, she will change us into `sams old pants`
    
### Option 2 (used in dynamic dialoge system triggered by code)
#### !! Can only be used in `Regression\Data\VillagerData.json`!! 
* Change can be triggered with a question by Placeholder `$GETTING_CHANGED_DIALOG$`
* These option needs one parameter
  * `\"npc name\"`        -> name of the npc whitch will give you a change
* The placeholder will be replaced with the text under `Diaper_Change_Dialog` you can find in `Regression\Data\ChangeData.json`
  * It generates a question with the npc name that makes it possible to ask multiple npc´s
#### !! Important !! 
* To use this the npc, that triggered these dialouge, needs to have the responce id´s `Diaper_Change_Accept`, `Diaper_Change_Refuse` and `Diaper_Change_Followup` whitch will be need to insert to `Regression Dialogue\Dialogue\NPCs\` under `"Target: "Characters/Dialogue/Npc-Name"`
* Example:
```
"Changes": [
  {
    "LogName": "Abigail",
    "Action": "EditData",
    "Target": "Characters/Dialogue/Abigail",
    "Entries": {
      "Diaper_Change_Accept": "Text for Change Accept#$action DIAPER_CHANGE \"abigail\" \"joja diaper\" \"stinky pants\"#$h",
      "Diaper_Change_Refuse": "Text for Change Refuse",
      "Diaper_Change_Followup": "Text for Change Followup"
    }
  }
]
```
  * Also the action Placeholder `DIAPER_CHANGE` needs to be in the response id to trigger the underwear change
