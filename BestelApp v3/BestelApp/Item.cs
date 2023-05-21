using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestelApp
{
    class Item
    {
        public int itemID { get; private set; }
        public decimal itemPrice { get; private set; }
        public string itemName { get; private set; }
        public string itemCategory { get; private set; }
        public int itemAantal { get;  set; }

        public Item(int ID, decimal Price, string Name, string Category,int Aantal)
        {
            itemID = ID;
            itemPrice = Price;
            itemName = Name;
            itemCategory = Category;
            itemAantal = Aantal;
        }
        public Item(int ID, decimal Price, string Name, string Category)
        {
            itemID = ID;
            itemPrice = Price;
            itemName = Name;
            itemCategory = Category;
        }
    }
}
