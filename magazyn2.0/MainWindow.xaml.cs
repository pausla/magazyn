using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Oracle.ManagedDataAccess.Client; //biblioteki oracle
using Oracle.ManagedDataAccess.Types;
using System.Configuration;

namespace magazyn2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OracleConnection con = null; // inicjalizacja obiektu con klasy oracleconnection przed otwarciem polczenienia
        public MainWindow()
        {
            this.SetConnection(); //wskaznik do funkcji otwierajacej polaczenie z baza 
            InitializeComponent();
        }

        private void UpdateDataGrid()
        {
            OracleCommand cmd = con.CreateCommand(); //tworzenie zmiennej do komendy sql
            cmd.CommandText = "SELECT EMPLOYEE_ID, LAST_NAME, EMAIL, HIRE_DATE, JOB_ID  FROM EMPLOYEES ORDER BY HIRE_DATE DESC"; 
            //tekst komendy sql
            cmd.CommandType = CommandType.Text;
            OracleDataReader dr = cmd.ExecuteReader(); //poczatek odczytu danych
            DataTable dt = new DataTable(); //tworze nowy obiekt DataTable
            dt.Load(dr); //tabela dt ma zaladowane dane dr pobrane z bazy danych
            MyDataGrid.ItemsSource = dt.DefaultView; //wyswietlenie tabeli w kontrolce datagrid 
            dr.Close(); // koniec odczytu
        }

        private void SetConnection()
        {
            String connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString; //przypisanie do zmiennej
            // connectionString adresu hosta hasla bazy danych z myConnectionstring (App.config)
            con = new OracleConnection(connectionString);//konstruktor, stworzenie obiektu con i przypisanie mu wartosci zmiennej connectionstring 
            try //sprawdzam czy polaczenie otwarte
            {
                con.Open();
            }
            catch (Exception exp) {

                MessageBox.Show("nie udalo sie polaczyc"); //wyrzuca wyjatek
                
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) //laduje zainicjalizowane dane gotowe do interakcji do grid
        {
            this.UpdateDataGrid(); //wskaznik do metody updatedatagrid
        }

        private void Window_Closed(object sender, EventArgs e) //zamykanie polaczenia przy zamknieciu okna
        {
            con.Close();
        }

        private void add_btn_Click(object sender, RoutedEventArgs e) //dodawanie wiersza po kliknieciu
        {
            String sql = "INSERT INTO EMPLOYEES(EMPLOYEE_ID, LAST_NAME, EMAIL, HIRE_DATE, JOB_ID )" + //komenda sql wklejenie wiersza do bazy
                "VALUES(:EMPLOYEE_ID, :LAST_NAME, :EMAIL, :HIRE_DATE, :JOB_ID)"; //przypisanie wartosci do kolumn
            this.AUD(sql, 0); //wskaznik do metody add update delete switch case 0
            add_btn.IsEnabled = false; //przycisk jest zablokowany
            update_btn.IsEnabled = true; //odblokowany
            delete_btn.IsEnabled = true; //odbokowany

        }

        private void update_btn_Click(object sender, RoutedEventArgs e) //aktualizacja wiersza
        {
            String sql = "UPDATE EMPLOYEES SET LAST_NAME =:LAST_NAME, EMAIL =:EMAIL, HIRE_DATE =:HIRE_DATE WHERE EMPLOYEE_ID =:EMPLOYEE_ID";
            this.AUD(sql, 1); //wskaznik do AUD
            this.UpdateDataGrid(); //zaktualizuj tabele, wskaznik do funcji
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e) //usuwanie wiersza z tabeli o okreslonym primary key
        {
            String sql = "DELETE FROM EMPLOYEES WHERE EMPLOYEE_ID =:EMPLOYEE_ID"; 
            this.AUD(sql, 2);
            this.resetAll(); //wyczyszczenie text boxow
        }

        private void resetAll() //czyszczenie kontrolek, zmiana stanu przyciskow
        {
            employee_id_txtbx.Text = "";
            email_txtbx.Text = "";
            last_name_txtbx.Text = "";
            job_id_txtbx.Text = "";
            hire_date_picker.SelectedDate = null;

            add_btn.IsEnabled = true;
            update_btn.IsEnabled = false;
            delete_btn.IsEnabled = false;

        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            this.resetAll();
        }
        private void AUD(String sql_stmt, int state)
        {
            String msg = ""; //zmienna msg
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = sql_stmt; //tekst komendy sql zapisywany do zmiennej
            cmd.CommandType = CommandType.Text;
            switch(state) //dodaje poszczegolne wartosci z textboxow jako parametry komendy sql do add, update i delete
            {
                case 0 :
                    msg = "Dodano nowy wiersz do bazy!";
                    cmd.Parameters.Add("EMPLOYEE_ID", OracleDbType.Int32, 6).Value = Int32.Parse(employee_id_txtbx.Text); //dodaj id z textbox itp
                    cmd.Parameters.Add("LAST_NAME", OracleDbType.Varchar2, 25).Value = last_name_txtbx.Text ;
                    cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2, 25).Value = email_txtbx.Text;
                    cmd.Parameters.Add("HIRE_DATE", OracleDbType.Date, 7).Value = hire_date_picker.SelectedDate;
                    cmd.Parameters.Add("JOB_ID", OracleDbType.Varchar2, 10).Value = job_id_txtbx.Text;

                    break;
                case 1 :
                    msg = "Zaktualizowano wiersz w bazie!";
                    cmd.Parameters.Add("LAST_NAME", OracleDbType.Varchar2, 25).Value = last_name_txtbx.Text ; //dodaj nowe dane z txtboxow
                    cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2, 25).Value = email_txtbx.Text;
                    cmd.Parameters.Add("HIRE_DATE", OracleDbType.Date, 7).Value = hire_date_picker.SelectedDate;
                    cmd.Parameters.Add("EMPLOYEE_ID", OracleDbType.Int32, 6).Value = Int32.Parse(employee_id_txtbx.Text);
                    
                    break;
                case 2 :
                    msg = "Usunięto wiersz z bazy!";
                    cmd.Parameters.Add("EMPLOYEE_ID", OracleDbType.Int32, 6).Value = Int32.Parse(employee_id_txtbx.Text); 
                    //przypisanie wartosci wybranego employeeid
                    //do parametrow delete
                    break;
            }
            try
            {
                int n = cmd.ExecuteNonQuery(); //executenonquery wykonuje zapytanie do bazy i zwraca liczbe zmienionych rzedow
                    if (n>0) //jesli 1 zapytanie sql zostalo wykonane
                    {
                        MessageBox.Show(msg); //pokaz wiadomosc
                        this.UpdateDataGrid(); //zaktualizuj grid

                    }

            }
            catch(Exception expe) //nie udalo sie wykonac komendy sql i polaczyc
            {
                MessageBox.Show("nie udalo sie");
                Console.WriteLine("Exception Message: " + expe.Message); //pokaz w konsoli blad
                Console.WriteLine("Exception Source: " + expe.Source); //pokaz zrodlo bledu
            }
        }

        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) //wyswietla wybrany rzad danych z datagrid w textbox
        {
            DataGrid dg = sender as DataGrid;
            DataRowView dr = dg.SelectedItem as DataRowView;
            if(dr != null)
            {
                employee_id_txtbx.Text = dr["EMPLOYEE_ID"].ToString();
                last_name_txtbx.Text = dr["LAST_NAME"].ToString();
                job_id_txtbx.Text = dr["JOB_ID"].ToString();
                email_txtbx.Text = dr["EMAIL"].ToString();
                hire_date_picker.SelectedDate = DateTime.Parse(dr["HIRE_DATE"].ToString());

                add_btn.IsEnabled = false;
                update_btn.IsEnabled = true;
                delete_btn.IsEnabled = true;
            }
        }
    }
}
