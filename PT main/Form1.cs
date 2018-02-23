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


    public partial class Form1 : Form
    {

        string connetionString = null;
        string sql = null;
        string sql2 = null;
        static string zm = null;
        //static DataSet dscof = null;

        SqlConnection connection;
        SqlCommand command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataSet ds = new DataSet();
        static DataSet cofnijSet = new DataSet();
        private BindingSource bindingSource1 = new BindingSource();

       // int i = 0;

        private login _ParentForm;
    
        public event EventHandler updateEvent;


        public Form1(string username, login parentForm)
        {
            InitializeComponent();

            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=PTproject;Integrated Security=True";
            sql = "select id, imie, nazwisko, indeks from studenci";
            connection = new SqlConnection(connetionString);


            List<string> nList = new List<string>();
            nList.Add("Moje Przedmioty");
            nList.Add("Przedmioty");
           // nList.Add("Obecnosci");
           // nList.Add("Moj Profil");
            listBox1.DataSource = nList;
            //Controls.Add(listBox1);


            details_btn.Visible = true;
            check_btn.Enabled = false;
            save_btn.Enabled = false;
            back_btn.Enabled = false;
            dataGridView1.Visible = true;
            dataGridView2.Visible = true;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            label1.Text = "Zalogowany: ";
            label2.Text = "Przemyslaw Walkowiak";
            //label3.Text = "Przedmiot";
            label4.Text = "";
            //label5.Text = "Widoki";
            label6.Text = "";
            label7.Text = "";
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //SZCZEGÓLY PRZEDMIOTU
        private void details_btnn_Click(object sender, EventArgs e)
        {
            DataSet sDs;
            DataSet sDs1;
            


            try
            {
                label4.Text = dataGridView1.SelectedCells[1].Value.ToString()   + " " + dataGridView1.SelectedCells[2].Value.ToString();
                label6.Text = "Wszystkie Zajecia";
                label7.Text = "Podsumowanie Obecnosci";

                sql = "select pz.id, p.nazwa as Przedmiot, p.typ as Rodzaj, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy, s.nr_sali, pz.dzien from przedmioty as p"
             + " join kierunki as k on k.id = p.kierunek "
             + " join wydzialy as w on w.id = k.wydzial_id "
             + " join kadra on kadra.id = p.prowadzacy_id "
             + " join plan_zajec as pz on pz.przedmiot_id=p.id "
             + " join sale as s on s.id=pz.sala_id"
             //+ " where p.prowadzacy_id =1 "
             // + " where p.id = 0";
             + " where p.id ="+dataGridView1.SelectedCells[0].Value.ToString();
                zm = dataGridView1.SelectedCells[0].Value.ToString();

                //SUM(CASE WHEN myColumn=1 THEN 1 ELSE 0 END)

                sql2 = "select s.imie+\' \'+s.nazwisko as student, s.indeks, count(*) as LiczbaZajec, SUM(CASE WHEN ls.obecny=1 THEN 1 ELSE 0 END) as Obecny from lista_obecnosci ls"
                + " join studenci as s on s.id=ls.student_id "
                + " join plan_zajec as pz on pz.id=ls.plan_id "
                + " join przedmioty as p on pz.przedmiot_id=p.id "
                + " where p.id =" + dataGridView1.SelectedCells[0].Value.ToString()
                + " group by s.indeks, s.imie+\' \'+s.nazwisko";

                //DealerName=ComboboxName.SelectedValue";

                connection = new SqlConnection(connetionString);

                try
                {
                    connection.Open();
                    command = new SqlCommand(sql, connection);
                    adapter.SelectCommand = command;
                    sDs = new DataSet();
                    cofnijSet = new DataSet();
                    dataGridView1.DataSource = null;
                    adapter.Fill(sDs, "przedmioty");
                    adapter.Fill(cofnijSet, "cofnij");                    
                    adapter.Dispose();
                    command.Dispose();

                    command = new SqlCommand(sql2, connection);
                    adapter.SelectCommand = command;
                    sDs1 = new DataSet();
                    adapter.Fill(sDs1, "frekwencja");
                    adapter.Dispose();
                    command.Dispose();


                    connection.Close();

                    dataGridView1.Visible = true;
                    dataGridView1.DataSource = sDs.Tables["przedmioty"];
                    dataGridView1.ReadOnly = true;
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    dataGridView2.Visible = true;
                    dataGridView2.DataSource = sDs1.Tables["frekwencja"];
                    dataGridView2.ReadOnly = true;
                    dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


                    details_btn.Enabled = false;
                    check_btn.Enabled = true;
                    save_btn.Enabled = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                //dataGridView1.DataSource = ds.Tables["pw"];
                //foreach (DataTable tables in ds.Tables)
                //{
                //    MessageBox.Show(tables.TableName);

                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show("Zaznacz Przedmiot do wyświetlenia!");
            }


        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].ErrorText = String.Empty;
        }


        //ZAPISZ OBECNOSC
        private void save_btn_Click(object sender, EventArgs e)
        {
            string StrQuery;
            try
            {
                //string MyConnection2 = "server=localhost;user id=root;password=;database=k";
                using (SqlConnection connection = new SqlConnection(connetionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            StrQuery = @"update lista_obecnosci set  obecny='" + dataGridView1.Rows[i].Cells["obecny"].Value.ToString() + "' where id='" + dataGridView1.Rows[i].Cells["ID"].Value.ToString() + "';";

                            command.CommandText = StrQuery;
                            command.ExecuteNonQuery();
                        }
                    }
                }


            }
            catch
            {

            }


            sql2 = "select s.imie+\' \'+s.nazwisko as student, s.indeks, count(*) as LiczbaZajec, SUM(CASE WHEN ls.obecny=1 THEN 1 ELSE 0 END) as Obecny from lista_obecnosci ls"
               + " join studenci as s on s.id=ls.student_id "
               + " join plan_zajec as pz on pz.id=ls.plan_id "
               + " join przedmioty as p on pz.przedmiot_id=p.id "
               + " where p.id =" + zm
               + " group by s.indeks, s.imie+\' \'+s.nazwisko";

            DataSet sDs2;
           connection = new SqlConnection(connetionString);

            try
            {
                connection.Open();

                command = new SqlCommand(sql2, connection);
                adapter.SelectCommand = command;
                sDs2 = new DataSet();
                adapter.Fill(sDs2, "frekwencja");
                adapter.Dispose();
                command.Dispose();


                connection.Close();

                dataGridView2.Visible = true;
                dataGridView2.DataSource = sDs2.Tables["frekwencja"];
                dataGridView2.ReadOnly = true;
                dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //MessageBox.Show("dataGridView2");

                }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }




        //SPRAWDZ OBECNOSC
        private void check_btn_Click(object sender, EventArgs e)
        {
           
           string numer_zajec = dataGridView1.SelectedCells[0].Value.ToString();

            DataSet sDs;
            // DataSet sDs1;
            back_btn.Enabled = true;
            views_btn.Enabled = false; 


            try
            {
                label4.Text = dataGridView1.SelectedCells[1].Value.ToString() + " " + dataGridView1.SelectedCells[2].Value.ToString();
                label6.Text = "Wszystkie Zajecia";
                label7.Text = "Podsumowanie Obecnosci";


                sql = "select ls.id, s.imie+\' \'+s.nazwisko as student, s.indeks as nr_indeksu, pz.dzien, ls.obecny from lista_obecnosci ls"
                    + " join studenci as s on s.id=ls.student_id "
                    + " join plan_zajec as pz on pz.id=ls.plan_id "
                    + " where pz.id=" + dataGridView1.SelectedCells[0].Value.ToString();

                connection = new SqlConnection(connetionString);

                try
                {
                    connection.Open();
                    command = new SqlCommand(sql, connection);
                    adapter.SelectCommand = command;
                    sDs = new DataSet();

                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    DataTable table = new DataTable();
                    table.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    //adapter.Fill(table);                
                    adapter.Fill(sDs, "check");
                    bindingSource1.DataSource = sDs.Tables["check"];
                    //bindingSource1.DataSource = table;

                    //adapter.Fill(sDs, "check");
                    adapter.Dispose();
                    command.Dispose();
                    connection.Close();


                    dataGridView1.Visible = true;
                    dataGridView1.DataSource = sDs.Tables["check"];
                    dataGridView1.ReadOnly = false;
                    foreach (DataGridViewColumn dc in dataGridView1.Columns)
                    {
                        if (dc.Index.Equals(4))
                        {
                            dc.ReadOnly = false;
                        }
                        else
                        {
                            dc.ReadOnly = true;
                        }
                    }
                    check_btn.Enabled = false;
                    save_btn.Enabled = true;
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Zaznacz Przedmiot do wyświetlenia!");
                MessageBox.Show(ex.ToString());
            }

        }



        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        //wczytywanie widokow
        private void views_btn_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = true;
            //Ta część kodu pobiera dane nt tabel
            string connetionString = null;
            string sql = null;
            dataGridView2.Visible = true;
            label7.Text = "";

            SqlCommand sCommand;
            SqlDataAdapter sAdapter;
            SqlCommandBuilder sBuilder;
            DataSet sDs;
            DataTable sTable;
            sTable = null;
            //string connetionString;
            SqlConnection connection;

            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=PTproject;Integrated Security=True";
            connection = new SqlConnection(connetionString);

            try
            {


                foreach (Object obj in listBox1.SelectedItems)
                {
                    if (obj.ToString() == "Moje Przedmioty")
                    {
                        label6.Text = "Moje Przedmioty";
                        sql = "select p.id, p.nazwa as Przedmiot, p.typ as Rodzaj, w.nazwa as Wydzial, k.nazwa as Kierunek, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy from przedmioty as p"
                        + " join kierunki as k on k.id = p.kierunek "
                        + " join wydzialy as w on w.id = k.wydzial_id "
                        + " join kadra on kadra.id = p.prowadzacy_id"
                        + " where p.prowadzacy_id =2";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "Moje Przedmioty");
                        sTable = sDs.Tables["Moje Przedmioty"];
                        connection.Close();
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.DataSource = sDs.Tables["Moje Przedmioty"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        details_btn.Enabled = true;
                        check_btn.Enabled = false;
                        save_btn.Enabled = false;

                    }
                    if (obj.ToString() == "Przedmioty")
                    {
                        label6.Text = "Wszystkie Przedmioty";
                        sql = "select p.id,p.nazwa as Przedmiot, p.typ as Rodzaj, w.nazwa as Wydzial, k.nazwa as Kierunek, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy from przedmioty as p"
                        + " join kierunki as k on k.id = p.kierunek "
                        +" join wydzialy as w on w.id = k.wydzial_id "
                        +" join kadra on kadra.id = p.prowadzacy_id";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "Przedmioty");
                        sTable = sDs.Tables["Przedmioty"];
                        connection.Close();
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.DataSource = sDs.Tables["Przedmioty"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        details_btn.Enabled = true;
                        check_btn.Enabled = false;
                        save_btn.Enabled = false;

                    }
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
      
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

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

        }

        private void back_btn_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = cofnijSet.Tables["cofnij"];
            check_btn.Enabled = true;
            back_btn.Enabled = false;
            views_btn.Enabled = true;
            save_btn.Enabled = false;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}