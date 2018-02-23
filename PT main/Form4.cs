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
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using ExcelDataReader;

namespace PT_main
{
    public partial class Form4 : Form
    {

        string connetionString = null;
        string sql = null;
        string sql2 = null;

        SqlConnection connection;
        SqlCommand command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataSet ds = new DataSet();

        public event EventHandler updateEvent;


        public Form4(string username)
        {
            InitializeComponent();

            List<string> nList = new List<string>();
            nList.Add("Studenci");
            nList.Add("Przedmioty");
            listBox1.DataSource = nList;
            label6.Text = "";
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ReadOnly = true;
            //dataGridView2.AllowUserToAddRows = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void views_btn_Click(object sender, EventArgs e)
        {
            string connetionString = null;
            string sql = null;
            SqlCommand sCommand;
            SqlDataAdapter sAdapter;
            SqlCommandBuilder sBuilder;
            DataSet sDs;
            //DataTable sTable;
            //sTable = null;
            //string connetionString;
            SqlConnection connection;

            connetionString = "Data Source=WHYNOT-KOMPUTER\\SQLEXPRESS;Initial Catalog=PTproject;Integrated Security=True";
            connection = new SqlConnection(connetionString);


            try
            {


                foreach (Object obj in listBox1.SelectedItems)
                {
                    if (obj.ToString() == "Studenci")
                    {

                        label6.Text = "Studenci";
                        sql = "select p.id, p.nazwa as Przedmiot, p.typ as Rodzaj, w.nazwa as Wydzial, k.nazwa as Kierunek, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy from przedmioty as p"
                        + " join kierunki as k on k.id = p.kierunek "
                        + " join wydzialy as w on w.id = k.wydzial_id "
                        + " join kadra on kadra.id = p.prowadzacy_id"
                        + " where p.prowadzacy_id =2";


                        sql = "select s.id, imie, nazwisko, email, indeks, w.nazwa as Wydzial, k.nazwa as Kierunek, k.tryb_stacjonarny, k.rok_rozpoczecia, s.aktywny as Status from studenci as s"
                            + " join zapisani_na_kierunek as zap on zap.student_id = s.id"
                            + " join kierunki as k on k.id = zap.kierunek_id "
                            + " join wydzialy as w on w.id = k.wydzial_id";

                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        ds = new DataSet();
                        sAdapter.Fill(ds, "Studenci");
                        //sTable = sDs.Tables["Moje Przedmioty"];
                        connection.Close();
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.DataSource = ds.Tables["Studenci"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    }
                    if (obj.ToString() == "Przedmioty")
                    {
                        sql = "select p.id,p.nazwa as Przedmiot, p.typ as Rodzaj, w.nazwa as Wydzial, k.nazwa as Kierunek, kadra.imie+\' \'+kadra.nazwisko as Prowadzacy from przedmioty as p"
                        + " join kierunki as k on k.id = p.kierunek "
                        + " join wydzialy as w on w.id = k.wydzial_id "
                        + " join kadra on kadra.id = p.prowadzacy_id";
                        connection.Open();
                        sCommand = new SqlCommand(sql, connection);
                        sAdapter = new SqlDataAdapter(sCommand);
                        sBuilder = new SqlCommandBuilder(sAdapter);
                        sDs = new DataSet();
                        sAdapter.Fill(sDs, "Przedmioty");
                        //sTable = sDs.Tables["Przedmioty"];
                        connection.Close();
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.DataSource = sDs.Tables["Przedmioty"];
                        dataGridView1.ReadOnly = true;
                        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                  

                    }
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void export_btn_Click(object sender, EventArgs e)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            Int16 i, j;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Add(misValue);

            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            
            xlWorkBook.SaveAs(@"d:\csharp.net-informations2.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);


            //Creae an Excel application instance
            Excel.Application excelApp = new Excel.Application();

            //Create an Excel workbook instance and open it from the predefined location
            Excel.Workbook excelWorkBook = excelApp.Workbooks.Open(@"d:\csharp.net-informations2.xls");
           // excelApp.Workbooks.Open(@"d:\Org2.xls");

            foreach (DataTable table in ds.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                excelWorkSheet.Name = table.TableName;

                for (int a = 1; a < table.Columns.Count + 1; a++)
                {
                    excelWorkSheet.Cells[1, a] = table.Columns[a - 1].ColumnName;
                }

                for (int b = 0; b < table.Rows.Count; b++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {
                        excelWorkSheet.Cells[b + 2, k + 1] = table.Rows[b].ItemArray[k].ToString();
                    }
                }
            }

            excelWorkBook.Save();
            excelWorkBook.Close();
            excelApp.Quit();




        }

        private void button1_Click(object sender, EventArgs e)
        {
            //dataGridView2.Visible = false;
            dataGridView2.DataSource = null;
            dataGridView2.ColumnCount = 9;


            Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Excel.Workbook workbook = app.Workbooks.Open(@"d:\csharp.net-informations2.xls");
            Excel.Worksheet worksheet = workbook.ActiveSheet;

            int rcount = worksheet.UsedRange.Rows.Count;
            
            int i = 0;
          
            for (; i+1 < rcount; i++)
            {
                dataGridView2.Columns[0].HeaderText = worksheet.Cells[1, 1].Value;
                dataGridView2.Columns[1].HeaderText = worksheet.Cells[1, 2].Value;
                dataGridView2.Columns[2].HeaderText = worksheet.Cells[1, 3].Value;
                dataGridView2.Columns[3].HeaderText = worksheet.Cells[1, 4].Value;
                dataGridView2.Columns[4].HeaderText = worksheet.Cells[1, 5].Value;
                dataGridView2.Columns[5].HeaderText = worksheet.Cells[1, 6].Value;
                dataGridView2.Columns[6].HeaderText = worksheet.Cells[1, 7].Value;
                dataGridView2.Columns[7].HeaderText = worksheet.Cells[1, 8].Value;
                dataGridView2.Columns[8].HeaderText = worksheet.Cells[1, 9].Value;
                // dataGridView2.Rows[0].Cells["Column1"].Value = worksheet.Cells[1, 1].Value;
                //ataGridView1.Rows[i].Cells["Column2"].Value = worksheet.Cells[i + 2, 2].Value;
                dataGridView2.Rows.Add(worksheet.Cells[i + 2, 1].Value, worksheet.Cells[i + 2, 2].Value,
                    worksheet.Cells[i + 2, 3].Value, worksheet.Cells[i + 2, 4].Value,worksheet.Cells[i + 2, 5].Value,
                    worksheet.Cells[i + 2, 6].Value, worksheet.Cells[i + 2, 7].Value, worksheet.Cells[i + 2, 8].Value,
                    worksheet.Cells[i + 2, 9].Value);
            }

           
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
