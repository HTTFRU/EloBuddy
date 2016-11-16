using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using System;

namespace HTTF_Riven_v2
{
    class ItemLogic
    {
        public static Item Hydra;

        public static void Init()
        {
            UpdateItems();
            Shop.OnBuyItem += delegate { UpdateItems(); };
            Shop.OnSellItem += delegate { UpdateItems(); };
            Shop.OnUndo += delegate { UpdateItems(); };
            Player.OnSwapItem += delegate { UpdateItems(); };
        }

        public static void UpdateItems()
        {
            Core.DelayAction(() =>
            {
                var id =
                    Player.Instance.InventoryItems.FirstOrDefault(
                        a => a.Id == ItemId.Ravenous_Hydra_Melee_Only || a.Id == ItemId.Tiamat_Melee_Only);
                if (id == null)
                {
                    Hydra = null;
                    return;
                }
                Hydra = new Item(id.Id, 300);
            }, 200);
        }
    }
}