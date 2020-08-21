using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Craft Car Chassis", "WhiteThunder", "1.0.0")]
    [Description("Allows players to craft a modular car chassis at a car lift using a UI.")]
    internal class CraftChassis : CovalencePlugin
    {
        #region Fields

        private static CraftChassis PluginInstance;

        private CraftChassisConfig PluginConfig;

        private const string PermissionCraft2 = "craftchassis.2";
        private const string PermissionCraft3 = "craftchassis.3";
        private const string PermissionCraft4 = "craftchassis.4";
        private const string PermissionFree = "craftchassis.free";

        private const string ChassisPrefab2 = "assets/content/vehicles/modularcar/car_chassis_2module.entity.prefab";
        private const string ChassisPrefab3 = "assets/content/vehicles/modularcar/car_chassis_3module.entity.prefab";
        private const string ChassisPrefab4 = "assets/content/vehicles/modularcar/car_chassis_4module.entity.prefab";
        private const string SpawnEffect = "assets/bundled/prefabs/fx/build/promote_toptier.prefab";

        private readonly Dictionary<BasePlayer, ModularCarGarage> PlayerLifts = new Dictionary<BasePlayer, ModularCarGarage>();
        private readonly ChassisUIManager UIManager = new ChassisUIManager();

        #endregion

        #region Hooks

        private void Init()
        {
            PluginInstance = this;
            PluginConfig = Config.ReadObject<CraftChassisConfig>();

            permission.RegisterPermission(PermissionCraft2, this);
            permission.RegisterPermission(PermissionCraft3, this);
            permission.RegisterPermission(PermissionCraft4, this);
            permission.RegisterPermission(PermissionFree, this);
        }

        private void Unload()
        {
            UIManager.DestroyAllUIs();
            PluginInstance = null;
        }

        void OnLootEntity(BasePlayer player, ModularCarGarage carLift)
        {
            if (carLift == null) return;
            if (carLift.carOccupant == null)
            {
                PlayerLifts.Add(player, carLift);
                UIManager.MaybeSendPlayerUI(player);
            }
            else
                UIManager.DestroyPlayerUI(player);
        }

        void OnPlayerLootEnd(PlayerLoot inventory)
        {
            var player = inventory.GetComponent<BasePlayer>();
            if (player == null) return;
            PlayerLifts.Remove(player);
            UIManager.DestroyPlayerUI(player);
        }

        #endregion

        #region Commands

        [Command("craftchassis.ui")]
        private void CraftChassisUICommand(IPlayer player, string cmd, string[] args)
        {
            if (player.IsServer || args.Length < 1) return;

            int numSockets;
            if (!int.TryParse(args[0], out numSockets)) return;

            var maxAllowedSockets = GetMaxAllowedSockets(player);
            if (numSockets < 2 || numSockets > maxAllowedSockets) return;

            ItemCost itemCost = null;
            if (!CanPlayerCreateChassis(player, numSockets, ref itemCost)) return;

            var basePlayer = player.Object as BasePlayer;
            ModularCarGarage carLift;
            if (!PlayerLifts.TryGetValue(basePlayer, out carLift) || carLift.carOccupant != null) return;

            var car = SpawnChassis(carLift, numSockets);
            if (car == null) return;

            if (PluginConfig.EnableEffects)
                Effect.server.Run(SpawnEffect, car.transform.position);

            if (itemCost != null && itemCost.Amount > 0)
            {
                var itemid = ItemManager.itemDictionaryByName[itemCost.ItemShortName].itemid;
                basePlayer.inventory.Take(null, itemid, itemCost.Amount);
                basePlayer.Command("note.inv", itemid, -itemCost.Amount);
            }
        }

        #endregion

        #region Helper Methods

        private ModularCar SpawnChassis(ModularCarGarage carLift, int numSockets)
        {
            var prefab = GetChassisPrefab(numSockets);

            var position = carLift.GetNetworkPosition() + Vector3.up * 0.7f;
            var rotation = Quaternion.Euler(0, carLift.GetNetworkRotation().eulerAngles.y - 90, 0);

            var car = GameManager.server.CreateEntity(prefab, position, rotation) as ModularCar;
            if (car == null) return null;
            car.Spawn();

            return car;
        }

        private string GetChassisPrefab(int numSockets)
        {
            if (numSockets == 4)
                return ChassisPrefab4;
            else if (numSockets == 3)
                return ChassisPrefab3;
            else
                return ChassisPrefab2;
        }

        private int GetMaxAllowedSockets(IPlayer player)
        {
            if (player.HasPermission(PermissionCraft4))
                return 4;
            else if (player.HasPermission(PermissionCraft3))
                return 3;
            else if (player.HasPermission(PermissionCraft2))
                return 2;
            else
                return 0;
        }

        private bool CanPlayerCreateChassis(IPlayer player, int numSockets, ref ItemCost itemCost)
        {
            if (player.HasPermission(PermissionFree)) return true;

            var playerInventory = (player.Object as BasePlayer).inventory;
            itemCost = GetCostForSockets(numSockets);
            return playerInventory.GetAmount(ItemManager.itemDictionaryByName[itemCost.ItemShortName].itemid) >= itemCost.Amount;
        }

        private ItemCost GetCostForSockets(int numSockets)
        {
            if (numSockets == 4)
                return PluginConfig.ChassisCostMap.ChassisCost4;
            if (numSockets == 3)
                return PluginConfig.ChassisCostMap.ChassisCost3;
            else
                return PluginConfig.ChassisCostMap.ChassisCost2;
        }

        #endregion

        #region UI

        internal class ChassisUIManager
        {
            private const string PanelBackgroundColor = "1 0.96 0.88 0.15";
            private const string TextColor = "0.97 0.92 0.88 1";
            private const string DisabledLabelTextColor = "0.75 0.42 0.14 1";
            private const string ButtonColor = "0.44 0.54 0.26 1";
            private const string DisabledButtonColor = "0.25 0.32 0.19 0.7";

            private const string CraftChassisUIName = "CraftChassis";
            private const string CraftChassisUIHeaderName = "CraftChassis.Header";

            private readonly List<BasePlayer> PlayersWithUIs = new List<BasePlayer>();

            public void DestroyAllUIs()
            {
                var playerList = new BasePlayer[PlayersWithUIs.Count];
                PlayersWithUIs.CopyTo(playerList, 0);

                foreach (var player in playerList)
                    DestroyPlayerUI(player);
            }

            public void DestroyPlayerUI(BasePlayer player)
            {
                if (PlayersWithUIs.Contains(player))
                {
                    CuiHelper.DestroyUi(player, CraftChassisUIName);
                    PlayersWithUIs.Remove(player);
                }
            }

            private CuiLabel CreateCostLabel(BasePlayer player, bool freeCrafting, int maxAllowedSockets, int numSockets)
            {
                var freeLabel = PluginInstance.GetMessage(player.IPlayer, "UI.CostLabel.Free");

                string text = freeLabel;
                string color = TextColor;

                if (numSockets > maxAllowedSockets)
                {
                    text = PluginInstance.GetMessage(player.IPlayer, "UI.CostLabel.NoPermission");
                    color = DisabledLabelTextColor;
                }
                else if (!freeCrafting)
                {
                    var itemCost = PluginInstance.GetCostForSockets(numSockets);
                    var itemDefinition = ItemManager.itemDictionaryByName[itemCost.ItemShortName];

                    if (itemCost.Amount > 0)
                    {
                        text = $"{itemCost.Amount} {itemDefinition.displayName.translated}";

                        if (player.inventory.GetAmount(itemDefinition.itemid) < itemCost.Amount)
                            color = DisabledLabelTextColor;
                    }
                }

                int offsetMinX = 8 + (numSockets - 2) * 124;
                int offsetMaxX = 124 + (numSockets - 2) * 124;
                int offsetMinY = 43;
                int offsetMaxY = 58;

                return new CuiLabel
                {
                    Text =
                    {
                        Text = text,
                        Color = color,
                        Align = TextAnchor.MiddleCenter,
                        FontSize = 11,
                    },
                    RectTransform =
                    {
                        AnchorMin = "0 0",
                        AnchorMax = "0 0",
                        OffsetMin = $"{offsetMinX} {offsetMinY}",
                        OffsetMax = $"{offsetMaxX} {offsetMaxY}"
                    }
                };
            }

            private CuiButton CreateCraftButton(BasePlayer player, bool freeCrafting, int maxAllowedSockets, int numSockets)
            {
                var color = ButtonColor;

                if (numSockets > maxAllowedSockets)
                {
                    color = DisabledButtonColor;
                }
                else if (!freeCrafting)
                {
                    var itemCost = PluginInstance.GetCostForSockets(numSockets);
                    var itemDefinition = ItemManager.itemDictionaryByName[itemCost.ItemShortName];

                    if (itemCost.Amount > 0 && player.inventory.GetAmount(itemDefinition.itemid) < itemCost.Amount)
                        color = DisabledButtonColor;
                }

                int offsetMinX = 8 + (numSockets - 2) * 124;
                int offsetMaxX = 124 + (numSockets - 2) * 124;
                int offsetMinY = 8;
                int offsetMaxY = 40;

                return new CuiButton
                {
                    Text = {
                        Text = PluginInstance.GetMessage(player.IPlayer, "UI.ButtonText.Sockets", numSockets),
                        Color = TextColor,
                        Align = TextAnchor.MiddleCenter
                    },
                    Button =
                    {
                        Color = color,
                        Command = $"craftchassis.ui {numSockets}"
                    },
                    RectTransform =
                    {
                        AnchorMin = "0 0",
                        AnchorMax = "0 0",
                        OffsetMin = $"{offsetMinX} {offsetMinY}",
                        OffsetMax = $"{offsetMaxX} {offsetMaxY}"
                    }
                };
            }

            public void MaybeSendPlayerUI(BasePlayer player)
            {
                if (PlayersWithUIs.Contains(player)) return;

                var maxAllowedSockets = PluginInstance.GetMaxAllowedSockets(player.IPlayer);
                if (maxAllowedSockets == 0) return;

                var freeCrafting = player.IPlayer.HasPermission(PermissionFree);

                var cuiElements = new CuiElementContainer
                {
                    {
                        new CuiPanel
                        {
                            Image = new CuiImageComponent { Color = PanelBackgroundColor },
                            RectTransform =
                            {
                                AnchorMin = "1 0",
                                AnchorMax = "1 0",
                                OffsetMin = "-447.5 431",
                                OffsetMax = "-67.5 495",
                            }
                        },
                        "Hud.Menu",
                        CraftChassisUIName
                    },
                    {
                        new CuiPanel
                        {
                            Image = new CuiImageComponent { Color = PanelBackgroundColor },
                            RectTransform =
                            {
                                AnchorMin = "0 1",
                                AnchorMax = "0 1",
                                OffsetMin = "0 3",
                                OffsetMax = "380 24"
                            }
                        },
                        CraftChassisUIName,
                        CraftChassisUIHeaderName
                    },
                    {
                        new CuiLabel
                        {
                            RectTransform =
                            {
                                AnchorMin = "0 0",
                                AnchorMax = "1 1",
                                OffsetMin = "10 0",
                                OffsetMax = "0 0"
                            },
                            Text =
                            {
                                Text = PluginInstance.GetMessage(player.IPlayer, "UI.Header").ToUpperInvariant(),
                                Align = TextAnchor.MiddleLeft,
                                FontSize = 13
                            }
                        },
                        CraftChassisUIHeaderName
                    },
                    { CreateCostLabel(player, freeCrafting, maxAllowedSockets, 2), CraftChassisUIName },
                    { CreateCraftButton(player, freeCrafting, maxAllowedSockets, 2), CraftChassisUIName },
                    { CreateCostLabel(player, freeCrafting, maxAllowedSockets, 3), CraftChassisUIName },
                    { CreateCraftButton(player, freeCrafting, maxAllowedSockets, 3), CraftChassisUIName },
                    { CreateCostLabel(player, freeCrafting, maxAllowedSockets, 4), CraftChassisUIName },
                    { CreateCraftButton(player, freeCrafting, maxAllowedSockets, 4), CraftChassisUIName },
                };

                CuiHelper.AddUi(player, cuiElements);
                PlayersWithUIs.Add(player);
            }
        }

        #endregion

        #region Configuration

        protected override void LoadDefaultConfig() => Config.WriteObject(new CraftChassisConfig(), true);

        internal class CraftChassisConfig
        {
            [JsonProperty("ChassisCost")]
            public ChassisCostMap ChassisCostMap = new ChassisCostMap();

            [JsonProperty("EnableEffects")]
            public bool EnableEffects = true;
        }

        internal class ChassisCostMap
        {
            [JsonProperty("2sockets")]
            public ItemCost ChassisCost2 = new ItemCost
            {
                ItemShortName = "metal.fragments",
                Amount = 200,
            };

            [JsonProperty("3sockets")]
            public ItemCost ChassisCost3 = new ItemCost
            {
                ItemShortName = "metal.fragments",
                Amount = 300,
            };

            [JsonProperty("4sockets")]
            public ItemCost ChassisCost4 = new ItemCost
            {
                ItemShortName = "metal.fragments",
                Amount = 400,
            };
        }

        internal class ItemCost
        {
            [JsonProperty("ItemShortName")]
            public string ItemShortName;

            [JsonProperty("Amount")]
            public int Amount;
        }

        #endregion

        #region Localization

        private string GetMessage(IPlayer player, string messageName, params object[] args)
        {
            var message = lang.GetMessage(messageName, this, player.Id);
            return args.Length > 0 ? string.Format(message, args) : message;
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["UI.Header"] = "Craft a chassis",
                ["UI.CostLabel.Free"] = "Free",
                ["UI.CostLabel.NoPermission"] = "No Permission",
                ["UI.ButtonText.Sockets"] = "{0} sockets",
            }, this, "en");
        }

        #endregion
    }
}
