**Craft Car Chassis** allows players to craft a modular car chassis at a car lift using a UI.

The UI appears only if the player has permission and is "looting" a car lift that currently has no car on it.

Also recommended: [Monument Lifts](https://umod.org/plugins/monument-lifts) (adds public car lifts to select monuments).

## UI Screenshots

![Craft Chassis UI Buttons](https://i.imgur.com/3euUusz.png)

## Permissions

- `craftchassis.2` -- Allows crafting a chassis with 2 sockets.
- `craftchassis.3` -- Allows crafting a chassis with 2-3 sockets.
- `craftchassis.4` -- Allows crafting a chassis with 2-4 sockets.
- `craftchassis.free` -- Allows crafting a chassis for free (no resource cost).

## Configuration

Default configuration:
```json
{
  "ChassisCost": {
    "2sockets": {
      "Amount": 200,
      "ItemShortName": "metal.fragments"
    },
    "3sockets": {
      "Amount": 300,
      "ItemShortName": "metal.fragments"
    },
    "4sockets": {
      "Amount": 400,
      "ItemShortName": "metal.fragments"
    }
  },
  "EnableEffects": true
}
```

- `ChassisCost` -- Setting a particular chassis cost `Amount` to `0` will make it free for everyone who has permission.
- `EnableEffects` (`true` or `false`) -- Whether to play an effect when a chassis is crafted.

## Localization

```json
{
  "UI.Header": "Craft a chassis",
  "UI.CostLabel.Free": "Free",
  "UI.CostLabel.NoPermission": "No Permission",
  "UI.ButtonText.Sockets": "{0} sockets"
}
```
