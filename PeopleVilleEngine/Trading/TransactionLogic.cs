using PeopleVilleEngine.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleVilleEngine.Trading
{
    public static class TransactionLogic
    {
        public static bool TransferMoney(ITrader from, ITrader to, int amount)
        {
            if (amount <= 0 || from.Money < amount) return false;
            from.Money -= amount;
            to.Money += amount;
            return true;
        }

        public static bool TransferItem(ITrader from, ITrader to, Item item)
        {
            if (!from.Inventory.Contains(item)) return false;
            from.Inventory.Remove(item);
            to.Inventory.Add(item);
            return true;
        }
    }
}

// money/item transactions
// easy to demo in console
