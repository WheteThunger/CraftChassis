**Craft Car Chassis** allows players to craft a modular car chassis at a car lift using a UI.

The UI appears only if the player has permission and is "looting" a car lift that currently has no car on it.

Also recommended: [Monument Lifts](https://umod.org/plugins/monument-lifts) (adds public car lifts to select monuments).

## UI Screenshots

![Craft Chassis UI Buttons](https://i.imgur.com/3euUusz.png)

## Permissions

- `craftchassis.2` -- Allows crafting a chassis with 2 sockets.
- `craftchassis.3` -- Allows crafting a chassis with 2-3 sockets.
- `craftchassis.4` -- Allows crafting a chassis with 2-4 sockets.
- `craftchassis.free` -- Allows crafting a chassis for free (no resource cost). Note: The player still requires the above permissions to determine which ones they can craft.
- `craftchassis.fuel` -- Automatically adds fuel to any chassis the player crafts.

## Configuration

Default configuration:
```json
{
  "ChassisCost": {
    "2sockets": {
      "Amount": 200,
      "ItemShortName": "metal.fragments",
      "UseEconomics": false,
      "UseServerRewards": false
    },
    "3sockets": {
      "Amount": 300,
      "ItemShortName": "metal.fragments",
      "UseEconomics": false,
      "UseServerRewards": false
    },
    "4sockets": {
      "Amount": 400,
      "ItemShortName": "metal.fragments",
      "UseEconomics": false,
      "UseServerRewards": false
    }
  },
  "FuelAmount": 0,
  "EnableEffects": true,
  "SetOwner": false
}
```

- `ChassisCost` -- Setting a particular chassis cost `Amount` to `0` will make it free for everyone who has permission.
  - `Amount` -- Amount of item, Economics balance or Server Rewards points to charge the player for this chassis.
  - `ItemShortName` -- Short name of item to charge for, such as "metal.fragments".
  - `UseEconomics` (`true` or `false`) -- While `true`, players can only purchase this chassis with their [Economics](https://umod.org/plugins/economics) balance.
  - `UseServerRewards` (`true` or `false`) -- While `true`, players can only purchase this chassis with their [Server Rewards](https://umod.org/plugins/server-rewards) points.
- `FuelAmount` -- The amount of low grade fuel to add to the fuel tank (`-1` for max stack size). Only applies when the player has the `craftchassis.fuel` permission.
- `EnableEffects` (`true` or `false`) -- Whether to play an effect when a chassis is crafted.
- `SetOwner` (`true` or `false`) -- Whether to set the `OwnerID` of the chassis to the Steam ID of the player that crafted it. Setting `OwnerID` will allow various plugins to recognize cars spawned by this plugin so they can enable certain features (such as being able to pick up the car). This is off by default since there is no predicting how another plugin might behave when `OwnerID` is set (depends on which plugins you are running).

## Localization
