using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections;

namespace PT_main
{

    public partial class Form2: Form
    {
        public event EventHandler updateEvent;
        public Form2(string username)
        {
            InitializeComponent();
            List<string> nList = new List<string>();
            nList.Add("Studenci");
            nList.Add("Przedmioty");
            nList.Add("Obecnosci");
            nList.Add("Moj Profil");
            listBox1.DataSource = nList;
            //Controls.Add(listBox1);
            //delete_btn.enable = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Controls.Add(listBox1);
            listBox1.Items.Add("Sunday");
            listBox1.Items.Add("Monday");
            listBox1.Items.Add("Tuesday");
            listBox1.Items.Add("Wednesday");
            listBox1.Items.Add("Thursday");
            listBox1.Items.Add("Friday");
            listBox1.Items.Add("Saturday");
            listBox1.SelectionMode = SelectionMode.MultiSimple;



            //Ta część kodu pobiera dane nt tabel
            string connetionString = null;
            string sql = null;

            SqlConnection connection;
            SqlCommand command;
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();

            int i = 0;

            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=uczelnia;Integrated Security=True";
            sql = "select id, imie, nazwisko, indeks from studenci";
            connection = new SqlConnection(connetionString);

            try
            {
                connection.Open();
                command = new SqlCommand(sql, connection);
                adapter.SelectCommand = command;
                adapter.Fill(ds, "Studenci");
                adapter.Dispose();
                command.Dispose();
                connection.Close();




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



        }

        private void delete_btn_Click(object sender, EventArgs e)
        {
            ////Wczytanie listy na listbox
            //List<string> nList = new List<string>();
            //nList.Add("Studenci");
            //nList.Add("Przedmioty");
            //nList.Add("Obecnosci");
            //nList.Add("Moj Profil");
            //listBox1.DataSource = nList;
          


        }

        private void edit_btn_Click(object sender, EventArgs e)
        {
            

        }

        private void new_btn_Click(object sender, EventArgs e)
        {
            //ten kod wyciaga dane z kolumny imie tabela studenci
            string connetionString = null;
            SqlConnection connection;
            SqlCommand command;
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            int i = 0;
            string sql = null;
            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=uczelnia;Integrated Security=True";
            sql = "select id, imie, nazwisko, indeks from studenci";
            connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                command = new SqlCommand(sql, connection);
                adapter.SelectCommand = command;
                adapter.Fill(ds);
                adapter.Dispose();
                command.Dispose();
                connection.Close();
                listBox1.DataSource = ds.Tables[0];
                listBox1.ValueMember = "imie";
                listBox1.DisplayMember = "nazwisko";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open connection ! ");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ListBox listBox1 = new ListBox();
            //listBox1.Location = new System.Drawing.Point(12, 12);
            //listBox1.Name = "ListBox1";
            //listBox1.Size = new System.Drawing.Size(245, 200);
            //listBox1.BackColor = System.Drawing.Color.Orange;
            //listBox1.ForeColor = System.Drawing.Color.Black;
            //listBox1.Items.Add("Mahesh Chand");
            //listBox1.Items.Add("Mike Gold");
            //listBox1.Items.Add("Praveen Kumar");
            //listBox1.Items.Add("Raj Beniwal");

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void load_btn_Click(object sender, EventArgs e)
        {
 
            //Ta część kodu pobiera dane nt tabel
            string connetionString = null;
            string sql = null;

            SqlCommand sCommand;
            SqlDataAdapter sAdapter;
            SqlCommandBuilder sBuilder;
            DataSet sDs;
            DataTable sTable;
            // string connetionString;
            SqlConnection connection;

            int i = 0;

            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=uczelnia;Integrated Security=True";
            connection = new SqlConnection(connetionString);

            try
            {


                foreach (Object obj in listBox1.SelectedItems)
                {
                    if (obj.ToString() == "Studenci")
                    {
                        sql = "select id, imie, nazwisko, indeks from studenci";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "studenci");
                        sTable = sDs.Tables["studenci"];
                        connection.Close();
                        dataGridView1.DataSource = sDs.Tables["studenci"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        //MessageBox.Show("Zostales ZH4xxorowany");
                    }
                    if (obj.ToString() == "Przedmioty")
                    {
                        sql = "select * from przedmioty";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "przedmioty");
                        sTable = sDs.Tables["przedmioty"];
                        connection.Close();
                        dataGridView1.DataSource = sDs.Tables["przedmioty"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                       // MessageBox.Show("Zostales ZH4xxorowany");
                    }
                    if (obj.ToString() == "Moj Profil")
                    {
                        sql = "select s.nr_sali as sala, p.nazwa as przedmiot, k.imie+\' \'+k.nazwisko as prowadzacy, pz.dzien, odbyte from plan_zajec pz"
                        + " join sale as s on s.id=pz.sala_id"
                        + " join przedmioty as p on p.id=pz.przedmiot_id"
                        + " join kadra as k on k.id=pz.prowadzacy_id";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "plan_zajec");
                        sTable = sDs.Tables["plan_zajec"];
                        connection.Close();
                        dataGridView1.DataSource = sDs.Tables["plan_zajec"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        // MessageBox.Show("Zostales ZH4xxorowany");
                    }
                    if (obj.ToString() == "Obecnosci")
                    {
                        sql = "select * from lista_obecnosci";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "lista_obecnosci");
                        sTable = sDs.Tables["lista_obecnosci"];
                        connection.Close();
                        dataGridView1.DataSource = sDs.Tables["lista_obecnosci"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        // MessageBox.Show("Zostales ZH4xxorowany");
                    }

          



                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



        }
    }
}