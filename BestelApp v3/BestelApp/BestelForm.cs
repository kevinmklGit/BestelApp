using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BestelApp
{
    public partial class BestelForm : Form
    {
        private SerialMessenger serialMessenger;
        private Timer readMessageTimer;
        MySqlHandler mysql;
        List<Item> curView;
        Order curOrder;

        bool connected = false;

        public BestelForm()
        {
            InitializeComponent();

            MessageBuilder messageBuilder = new MessageBuilder('#', '%');
            serialMessenger = new SerialMessenger("COM3", 9600, messageBuilder);

            readMessageTimer = new Timer();
            readMessageTimer.Interval = 10;
            readMessageTimer.Tick += new EventHandler(readMessageTimer_Tick);

            curView = new List<Item>();
            curOrder = new Order();
            //lokale database
            mysql = new MySqlHandler("Data Source=localhost;Initial Catalog=Proftaak;User ID=root;Password=");

            //online database
            //mysql = new MySqlHandler(
            //"Data Source=mysql1075.cp.hostnet.nl;" +
            //"Initial Catalog=db268987_databasewebsite;" +
            //"User id=u268987_admin;" +
            //"Password=dimitrifontysjan;");
            //"Data Source=localhost;Initial Catalog=Proftaak;User ID=root;Password="

            LoadCats("SELECT Naam FROM Categorie");
        }
        //messages lezen
        private void readMessageTimer_Tick(object sender, EventArgs e)
        {
            string[] messages = serialMessenger.ReadMessages();
            if (messages != null)
            {
                foreach (string message in messages)
                {
                    processReceivedMessage(message);
                }
            }
        }
        // ontvangen message van de arduino na betaling
        private void processReceivedMessage(string message)
        {
            if (message == "PAID")
            {
                MessageBox.Show("Betaling geslaagd! Uw eten komt eraan.", "Geslaagd", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                curOrder.TableNRCheck(Convert.ToInt32(numericUpDown1.Value));
                mysql.MakeOrder(curOrder);
                mysql.BestellingBestand(curOrder);
                foreach (Item item in curOrder.orderList)
                {
                    mysql.AddOrderProducts(item);
                }
                curOrder.orderList.Clear();
                lbOrderAantal.Items.Clear();
                lbOrder.Items.Clear();
                ReloadOrders();
                PayTime(true);
            }
            else if (message == "FAIL")
            {
                MessageBox.Show("Betaling geannuleerd. Probeer opnieuw aub.", "Geannuleerd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PayTime(true);
            }
            else
            {

            }
        }
        //laden van categoriën
        public void LoadCats(string query)
        {
            List<string> test = new List<string>();
            test = mysql.SingleStringList(query);
            foreach (string txt in test)
            {
                cbCategories.Items.Add(txt);
            }
            cbCategories.SelectedIndex = 0;
        }
        //get selected categorie
        private void cbCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadList(cbCategories.SelectedItem.ToString());
        }

        public void ReloadList(string cat)
        {
            lbItems.Items.Clear();
            curView.Clear();
            List<Item> prods = new List<Item>();
            prods = mysql.LoadProducts(cat);
            foreach (Item eh in prods)
            {
                lbItems.Items.Add(eh.itemName);
                curView.Add(eh);
            }
        }

        public void ReloadOrders()
        {
            lbOrder.Items.Clear();
            lbOrderAantal.Items.Clear();
            foreach (Item item in curOrder.orderList)
            {
                lbOrder.Items.Add(item.itemName);
                lbOrderAantal.Items.Add("Aantal:  " + item.itemAantal);

            }
            curOrder.CalcCost();
            lblPay.Text = curOrder.cost.ToString();
        }
        //producten toevoegen aan de bestellijst
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lbItems.SelectedIndex != -1)
            {
                foreach (Item item in curView)
                {
                    if (item.itemName == lbItems.SelectedItem.ToString())
                    {
                        //kijken of het product in de bestellijst staat, zoja telt hij het product erbij op
                        if (AddQuantity() == true)
                        {

                        }
                        else
                        {
                            Item newItem = new Item(item.itemID, item.itemPrice, item.itemName, item.itemCategory, Convert.ToInt32(nudAantal.Value));
                            curOrder.orderList.Add(newItem);
                        }
                    }
                }
                nudAantal.Value = 1;
                ReloadOrders();
            }
        }
        //aantal toevoegen aan het product in orderlijst
        private bool AddQuantity()
        {
            foreach (Item item in curOrder.orderList)
            {
                if(item.itemName == lbItems.SelectedItem.ToString()==true)
                {
                    item.itemAantal = item.itemAantal + Convert.ToInt32(nudAantal.Value);
                    return true;
                }
            }
            return false;
        }
        //Producten verwijderen in de bestellijst
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lbOrder.SelectedIndex != -1)
            {
                int itemIndex = 0;
                int i = 0;
                foreach (Item item in curOrder.orderList)
                {
                    if (item.itemName == lbOrder.SelectedItem.ToString())
                    {
                        itemIndex = i;
                    }
                    i++;
                }
                curOrder.orderList.RemoveAt(itemIndex);
                ReloadOrders();
            }
        }
        //connect met arduino
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                serialMessenger.Connect();
                lblStatus.Text = "O";
                readMessageTimer.Enabled = true;
                btnDisco.Enabled = true;
                btnConnect.Enabled = false;
                connected = true;
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.Message);
            }
        }
        //disconnenect met arduino
        private void btnDisco_Click(object sender, EventArgs e)
        {
            try
            {
                readMessageTimer.Enabled = false;
                serialMessenger.Disconnect();
                lblStatus.Text = "X";
                btnConnect.Enabled = true;
                btnDisco.Enabled = false;
                connected = false;
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message);
            }
        }
        //bestelknop
        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (curOrder.cost > 0 && connected == true)
            {
                serialMessenger.SendMessage("STARTPAY:" + (curOrder.cost*100).ToString());
                PayTime(false);
            }
        }

        public void PayTime(bool what)
        {
            btnAdd.Enabled = what;
            btnOrder.Enabled = what;
            btnRemove.Enabled = what;
            lbOrder.Enabled = what;
            lbItems.Enabled = what;
            cbCategories.Enabled = what;
            btnDisco.Enabled = what;
        }

        private void BestelForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                readMessageTimer.Enabled = false;
                serialMessenger.Disconnect();
                connected = false;
            }
            catch (Exception ex3)
            {
                MessageBox.Show(ex3.Message);
            }
        }
    }
}
