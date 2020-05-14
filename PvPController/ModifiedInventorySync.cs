using CustomWeaponAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;

namespace PvPController
{
    struct ModifiedSlot
    {
        public StorageTypes.Weapon item;
        public int SlotIndex;
    }

    class ModifiedInventorySync
    {
        static ModifiedSlot? GetModifiedSlot(Dictionary<int, StorageTypes.Weapon> specs, Inventory.InventorySlot slot)
        {
            if (slot.Item.netID != 1 && specs.ContainsKey(slot.Item.netID))
            {
                StorageTypes.Weapon item = specs[slot.Item.netID];
                return new ModifiedSlot()
                {
                    item = item,
                    SlotIndex = slot.SlotIndex,
                };
            }
            else
            {
                return null;
            }
        }

        static IEnumerable<ModifiedSlot> GetModified(Dictionary<int, StorageTypes.Weapon> specs, IEnumerable<Inventory.InventorySlot> inventory)
        {
            return inventory
                .Select(slot => GetModifiedSlot(specs, slot))
                .Where(slot => slot.HasValue)
                .Select(slot => slot.Value);
        }

        /// <summary>
        /// Fills empty slots with a dummy item
        /// </summary>
        /// <param name="player"></param>
        static void FillEmpty(Player player, IEnumerable<Inventory.InventorySlot> inventory)
        {
            Item dummy = new Item();
            dummy.SetDefaults(1); // Iron Pickaxe
            for (int i = 0; i <= PvPController.MAX_SLOT_ID; i++)
            {
                if (Inventory.GetItem(inventory, i)?.netID == 0)
                {
                    DataSender.ForceServerItem(player, i, dummy);
                }
            }
        }

        /// <summary>
        /// Unfills empty slots with a dummy item
        /// </summary>
        /// <param name="player"></param>
        static void UnfillEmpty(Player player, IEnumerable<Inventory.InventorySlot> inventory)
        {
            Console.WriteLine($"Unfilling");
            Item empty = new Item();
            empty.SetDefaults(0);
            for (int i = 0; i <= PvPController.MAX_SLOT_ID; i++)
            {
                if (Inventory.GetItem(inventory, i)?.netID == 1)
                {
                    DataSender.ForceServerItem(player, i, empty);
                }
            }
        }

        /// <summary>
        /// Clears all slots in a players inventory that contain a modified item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="modifiedSlots"></param>
        static void ClearModified(Player player, List<ModifiedSlot> modifiedSlots)
        {
            Item empty = new Item();
            empty.SetDefaults(0);
            foreach (var slot in modifiedSlots)
            {   
                Console.WriteLine($"Clearing modified slot {slot.SlotIndex}.");
                DataSender.ForceServerItem(player, slot.SlotIndex, empty);
            }
        }

        static async Task DropModified(Player player, List<ModifiedSlot> modifiedSlots)
        {
            var hotbar = modifiedSlots.Where(slot => slot.SlotIndex <= 9); // Hotbar is picked up in normal order
            var storage = modifiedSlots.Where(slot => slot.SlotIndex > 9).Reverse(); // Storage is picked up in reverse order
            var combined = hotbar.Concat(storage);
            foreach (var modifiedSlot in combined)
            {
                Console.WriteLine($"Dropping {modifiedSlot.item.netID} Damage: {(ushort)modifiedSlot.item.currentDamage} vs {(ushort)modifiedSlot.item.baseDamage}");
                CustomWeaponDropper.DropItem(player.TshockPlayer, new CustomWeapon() {
                    ItemNetId = (short) modifiedSlot.item.netID,
                    Stack = 1,
                    Damage = (ushort) modifiedSlot.item.currentDamage,
                    ShootSpeed = modifiedSlot.item.currentVelocity,
                });
                await Task.Delay(TimeSpan.FromMilliseconds(2));
            }
        }

        internal static async Task ForceModifications(Player player, Dictionary<int, StorageTypes.Weapon> specs)
        {
            try
            {
                DataSender.ForceClientSSC(player, true);
                Console.WriteLine($"Syncing {player.TPlayer.name}");
                var inventory = Inventory.AsIEnumerable(player.TPlayer).ToList();
                var modified = GetModified(specs, inventory).ToList();
                FillEmpty(player, inventory);
                ClearModified(player, modified);
                await DropModified(player, modified);
                UnfillEmpty(player, inventory);
                DataSender.ForceClientSSC(player, false);
            } catch(Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}
