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
    

    public partial class Form3 : Form
    {

        public event EventHandler SelectedIndexChanged;
        public event EventHandler updateEvent;
        string sconnetionString = null;
        string ssql = null;
        string ssql2 = null;
        string ssql3 = null;
        static string zm = null;

        SqlConnection sconnection;
        SqlCommand scommand;
        SqlDataAdapter sadapter = new SqlDataAdapter();
        SqlCommandBuilder sbuilder;
        DataSet sds = new DataSet();
        DataSet sds2 = new DataSet();
        DataTable stable;
       // private BindingSource bindingSource1 = new BindingSource();

        private login _ParentForm;

        public Form3(string username, login parentForm)
        {
            InitializeComponent();

            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;
            dataGridView1.Enabled = true;
            dataGridView2.Enabled = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            label1.Text = "SZCEGÓŁY OBECNOŚCI";

            String student = null;
            sconnetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=PTproject;Integrated Security=True";
            ssql = "select imie, nazwisko, indeks from studenci"
                 + " where indeks=" + username;

            ssql2 = "select p.id, p.nazwa as Przedmiot, p.typ as Rodzaj, w.nazwa as Wydzial, k.nazwa as Kierunek,"
                + " kadra.imie+\' \'+kadra.nazwisko as Prowadzacy, count(*) as LiczbaZajec, SUM(CASE WHEN ls.obecny=1 THEN 1 ELSE 0 END) as Obecny from przedmioty as p"
                + " join kierunki as k on k.id = p.kierunek "
                + " join wydzialy as w on w.id = k.wydzial_id "
                + " join kadra on kadra.id = p.prowadzacy_id"
                + " join zapisani_na_przedmiot as zap on p.id = zap.przedmiot_id"
                + " join studenci as s on s.id=zap.student_id "
                + " join lista_obecnosci as ls on s.id=ls.student_id "
                + " where s.indeks =" + username
                + " group by p.id,p.nazwa, p.typ, w.nazwa, k.nazwa, kadra.imie+\' \'+kadra.nazwisko";
            //dodaj frekwencje

            ssql3 = "";

            try
            {                
                sconnection = new SqlConnection(sconnetionString);
                sconnection.Open();
                scommand = new SqlCommand(ssql, sconnection);
                sadapter = new SqlDataAdapter(scommand);
                sbuilder = new SqlCommandBuilder(sadapter);
                sds = new DataSet();
                sadapter.Fill(sds, "student");
                stable = sds.Tables["student"];
                sconnection.Close();
                            
                foreach (DataRow row in stable.Rows)
                {
                    if (row["indeks"].ToString() == username)
                    {
                        student = row["imie"].ToString() + " " + row["nazwisko"].ToString();
                    }
                }

                nazwa_studenta.Text = student;
            }

            catch { }

            try
            {
               // sconnection = new SqlConnection(sconnetionString);
                sconnection.Open();
                scommand = new SqlCommand(ssql2, sconnection);
                sadapter = new SqlDataAdapter(scommand);
                sbuilder = new SqlCommandBuilder(sadapter);
                sds2 = new DataSet();
                sadapter.Fill(sds2, "student przedmioty");
                stable = sds2.Tables["student przedmioty"];
                sconnection.Close();

                //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.DataSource = sds2.Tables["student przedmioty"];
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
      
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
 
        }

        
        private void nazwa_studenta_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Wyloguj_Click(object sender, EventArgs e)
        {
         
            try
            {
             ssql3 = "select  p.nazwa as Przedmiot, p.typ as Rodzaj, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy, s.nr_sali, pz.dzien from przedmioty as p"
             + " join kierunki as k on k.id = p.kierunek "
             + " join wydzialy as w on w.id = k.wydzial_id "
             + " join kadra on kadra.id = p.prowadzacy_id "
             + " join plan_zajec as pz on pz.przedmiot_id=p.id "
             + " join sale as s on s.id=pz.sala_id"
             //+ " where p.prowadzacy_id =1 "
             // + " where p.id = 0";
             + " where p.id =" + dataGridView1.SelectedCells[0].Value.ToString();
                zm = dataGridView1.SelectedCells[0].Value.ToString();

                sconnection = new SqlConnection(sconnetionString);

                try
                {
                    sconnection.Open();
                    scommand = new SqlCommand(ssql3, sconnection);
                    sadapter.SelectCommand = scommand;
                    sds = new DataSet();
                    sadapter.Fill(sds, "przedmioty");
                    sadapter.Dispose();
                    scommand.Dispose();
                    sconnection.Close();

                    dataGridView2.DataSource = sds.Tables["przedmioty"];
                    dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Zaznacz Przedmiot do wyświetlenia!");
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {
   
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

}
