using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BestelApp
{
    class Order
    {
        public DateTime orderTime { get; private set; }
        public decimal cost { get; private set; }
        public int tableNr { get; private set; }
        public List<Item> orderList { get; set; }

        public Order()
        {
            orderTime = DateTime.Now;
            orderList = new List<Item>();
        }
        //rekensom van de prijs van alle producten samen
        public void CalcCost()
        {
            decimal tempCost = 0.00m;
            foreach (Item item in orderList)
            {
                tempCost += item.itemPrice * item.itemAantal;
            }
            tempCost = Math.Round(tempCost, 2);
            cost = tempCost;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Ordertime: {0}", orderTime);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("Costs: {0}", cost);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("Tablenumber: {0}", tableNr);
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
        public void TableNRCheck(int tafelnummer)
        {
            tableNr = tafelnummer;
        }

    }
}
