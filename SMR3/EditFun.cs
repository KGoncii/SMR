using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace SMR3
{
    public class EditFun
    {
        public void Edit_Click(object sender, RoutedEventArgs e, MainWindow mainWindow)
        {
            DataGrid dataGrid = mainWindow.MyDataGrid;

            StringBuilder sb = new StringBuilder();

            foreach (Osoba item in dataGrid.ItemsSource)
            {
                if (item.IsChecked)
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                    {
                        DataSource = Dostep.DataSource,
                        InitialCatalog = Dostep.InitialCatalog,
                        UserID = Dostep.UserID,
                        Password = Dostep.Password,
                        ConnectTimeout = Dostep.ConnectTimeout,
                    };
                    string firma = item.Firma;
                    string pracownik = item.Pracownik;
                    string opis = item.Opis;
                    DateTime data = item.Data;
                    string czas = item.CzasString;
                    string sprzet = item.Sprzet;
                    string dojazd = item.DojazdString;
                    int ID = (int)item.ID;
                    string checkFirmaQuery = $"SELECT ID FROM Klienci WHERE Firma = '{firma}'";
                    int klientId = 0;

                    Int32 czasInt = 0;
                    if (!czas.Contains(',') && !czas.Contains('.'))
                        czas = czas + "0";
                    if (czas.EndsWith(",50 h") || czas.EndsWith(".50 h"))
                        czas = czas.Remove(czas.Length - 5) + "5";
                    if (czas.EndsWith(",5 h") || czas.EndsWith(".5 h"))
                        czas = czas.Remove(czas.Length - 4) + "5";
                    if (czas.EndsWith(",50h") || czas.EndsWith(".50h"))
                        czas = czas.Remove(czas.Length - 4) + "5";
                    if (czas.EndsWith(",5h") || czas.EndsWith(".5h"))
                        czas = czas.Remove(czas.Length - 3) + "5";
                    if (czas.EndsWith(",5") || czas.EndsWith(".5"))
                        czas = czas.Remove(czas.Length - 2) + "5";

                    if (czas.EndsWith(",00 h") || czas.EndsWith(".00 h"))
                        czas = czas.Remove(czas.Length - 5) + "0";
                    if (czas.EndsWith(",0 h") || czas.EndsWith(".0 h"))
                        czas = czas.Remove(czas.Length - 4) + "0";
                    if (czas.EndsWith(",00h") || czas.EndsWith(".00h"))
                        czas = czas.Remove(czas.Length - 4) + "0";
                    if (czas.EndsWith(",0h") || czas.EndsWith(".0h"))
                        czas = czas.Remove(czas.Length - 3) + "0";
                    if (czas.EndsWith(",h") || czas.EndsWith(".h") || czas.EndsWith(" h"))
                        czas = czas.Remove(czas.Length - 2) + "0";
                    if (czas.EndsWith("h"))
                        czas = czas.Remove(czas.Length - 1) + "0";
                    if (czas.EndsWith("h") && !czas.Contains(',') && !czas.Contains('.'))
                        czas = czas.Remove(czas.Length - 1);
                    try
                    {
                        czasInt = Int32.Parse(czas);
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Błąd: Podana czas nie spełnia standardów wejścia");
                        return;
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("Błąd: Podana czas jest zbyt duża lub zbyt mała, aby być przechowywana jako int.");
                        return;
                    }
                    Int32 dojazdInt = 0;
                    if (dojazd == "Zdalnie" || dojazd == "zdalnie")
                    {
                        dojazdInt = -1;
                    }
                    else
                    {
                        if (dojazd.EndsWith(",00 km"))
                            dojazd = dojazd.Remove(dojazd.Length - 6);
                        if (dojazd.EndsWith(".00 km"))
                            dojazd = dojazd.Remove(dojazd.Length - 6);
                        if (dojazd.EndsWith("00 km"))
                            dojazd = dojazd.Remove(dojazd.Length - 5);
                        if (dojazd.EndsWith(" km"))
                            dojazd = dojazd.Remove(dojazd.Length - 3);
                        if (dojazd.EndsWith("km"))
                            dojazd = dojazd.Remove(dojazd.Length - 2);
                        if (dojazd.EndsWith(",00"))
                            dojazd = dojazd.Remove(dojazd.Length - 3);
                        if (dojazd.EndsWith(".00"))
                            dojazd = dojazd.Remove(dojazd.Length - 3);
                        if (dojazd.EndsWith(",0"))
                            dojazd = dojazd.Remove(dojazd.Length - 2);
                        if (dojazd.EndsWith(".0"))
                            dojazd = dojazd.Remove(dojazd.Length - 2);
                        if (dojazd.EndsWith(".00km"))
                            dojazd = dojazd.Remove(dojazd.Length - 5);
                        if (dojazd.EndsWith(",00km"))
                            dojazd = dojazd.Remove(dojazd.Length - 5);
                        if (dojazd.EndsWith(".0km"))
                            dojazd = dojazd.Remove(dojazd.Length - 4);
                        if (dojazd.EndsWith(",0km"))
                            dojazd = dojazd.Remove(dojazd.Length - 4);
                        try
                        {
                            dojazdInt = Int32.Parse(dojazd);
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("Błąd: Podana długość nie spełnia standardów wejścia");
                            return;
                        }
                        catch (OverflowException)
                        {
                            MessageBox.Show("Błąd: Podana długość jest zbyt duża lub zbyt mała, aby być przechowywana jako int.");
                            return;
                        }
                    }
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(checkFirmaQuery, connection))
                            {
                                object result = command.ExecuteScalar();
                                if (result != null)
                                {
                                    klientId = Convert.ToInt32(result);
                                }
                                else
                                {
                                    MessageBoxResult odpowiedz = MessageBox.Show("Firma nie istnieje w bazie danych\nCzy chcesz ją dodać?", "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (odpowiedz == MessageBoxResult.Yes)
                                    {
                                        Window2 window2 = new Window2();
                                        window2.FirmaTextBox.Text = firma;
                                        window2.Show();
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Błąd podczas sprawdzania firmy: " + ex.Message);
                    }

                    string checkPracownikQuery = $"SELECT ID FROM Pracownicy WHERE Nazwisko = '{pracownik}'";
                    int pracownikId = 0;

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(checkPracownikQuery, connection))
                            {
                                object result = command.ExecuteScalar();
                                if (result != null)
                                {
                                    pracownikId = Convert.ToInt32(result);
                                }
                                else
                                {
                                    MessageBox.Show("Pracownik nie istnieje w bazie danych.");
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Błąd podczas sprawdzania pracownika: " + ex.Message);
                    }

                    string dataString;

                    try
                    {
                        dataString = data.ToString("yyyy-MM-dd");
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Błąd: Podana czas nie spełnia standardów wejścia");
                        return;
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("Błąd: Podana czas jest zbyt duża lub zbyt mała, aby być przechowywana jako int.");
                        return;
                    }

                    string updateQuery = $"UPDATE Uslugi SET ID_Klienta = {klientId}, ID_Pracownika = {pracownikId}, Opis = '{opis}', Sprzet ='{sprzet}', Data ='{dataString}', Czas ={(((decimal)czasInt) / 10).ToString(System.Globalization.CultureInfo.InvariantCulture)}, Dojazd ={dojazdInt} WHERE ID = {ID};";

                    sb.AppendLine($"Zapytanie SQL: {updateQuery}");
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                MessageBox.Show($"Zaktualizowano zamówienie o ID: {ID}.");

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Błąd podczas aktualizacji wiersza: " + ex.Message);
                    }
                }
            }
        }
    }
}
