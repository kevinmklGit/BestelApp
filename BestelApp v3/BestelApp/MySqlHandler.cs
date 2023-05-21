using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;

namespace BestelApp
{
    class MySqlHandler
    {
        private static string cString;
        public long order_id;
        public MySqlHandler(string connString)
        {
            cString = connString;
        }
        //haalt verschillende categorieën op
        public List<string> SingleStringList(string queryInput)
        {
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = cString;
            conn.Open();
            string query = queryInput;
            List<string> output = new List<string>();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    output.Add(name);
                }
            }
            conn.Close();
            return output;
        }
        //producten laden vanuit de database
        public List<Item> LoadProducts(string category)
        {
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = cString;
            conn.Open();
            string query = "SELECT ID, Prijs, naam, categorie_naam FROM Product WHERE categorie_naam = '" + category + "'";
            List<Item> output = new List<Item>();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int qID = reader.GetInt32(0);
                    decimal qPrice = reader.GetDecimal(1);
                    string qNaam = reader.GetString(2);
                    string qCategory = reader.GetString(3);
                    Item item = new Item(qID, qPrice, qNaam, qCategory);
                    output.Add(item);
                }
            }
            conn.Close();
            return output;
        }
        public void MakeOrder(Order order)
        {
            //Bestelling neerzetten in bestelling tabel
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = cString;
            conn.Open();
            string query = "INSERT INTO Bestelling (Order_tijd, Kosten,klant_tafelnummer) VALUES(@ordertijd,@kosten,@tafelnummer)";
            List<Item> output = new List<Item>();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@kosten",order.cost);
            cmd.Parameters.AddWithValue("@tafelnummer",order.tableNr);
            cmd.Parameters.AddWithValue("@ordertijd",order.orderTime);
            cmd.ExecuteNonQuery();
            order_id = cmd.LastInsertedId;
            conn.Close();

       }
        //bestelling verwerken in de koppeltabel zodat alle producten erin kunnen
        public void  AddOrderProducts(Item item)
        {
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = cString;
            conn.Open();
            string query = "INSERT INTO Overzicht (product_id,order_id,Aantal) VALUES(@productid,@orderid,@aantal)";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@productid", item.itemID);
            cmd.Parameters.AddWithValue("@orderid", Convert.ToInt32(order_id));
            cmd.Parameters.AddWithValue("@aantal", item.itemAantal);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        //bestelling in bestand plaatsen
        public void BestellingBestand(Order order)
        {
            using (StreamWriter file =
                new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"orderlist.txt", true))
            {
                file.WriteLine("OrderID: " + Convert.ToString(order_id));
                file.WriteLine(order.ToString());
            }
        }
    }
}
