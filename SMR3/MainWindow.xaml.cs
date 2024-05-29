using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Application = Microsoft.Office.Interop.Word.Application;



namespace SMR3
{
    public partial class MainWindow : System.Windows.Window
    {
        private MainWindow mainWindow;
        private DatabaseOperations dbOperations;
        private List<Employee> employeesList;
        private List<Client> clientsList;
        public ObservableCollection<Osoba> Items { get; set; }
        bool pom=false;

        public MainWindow()
        {
            InitializeComponent();
            this.mainWindow = this;
            LoadFirmyFromDatabase();
            LoadNazwiskaFromDatabase();
            menuComboBox.SelectedItem = menuComboBox.Items[0];
            string connectionString = Dostep.GetConnectionString();
            dbOperations = new DatabaseOperations(connectionString);
            if (dbOperations.TestConnection())
            {
                employeesList = dbOperations.LoadEmployeesFromDatabase();
                clientsList = dbOperations.LoadClientsFromDatabase();
                EmployeeComboBox1.ItemsSource = employeesList.Select(emp => $"{emp.Imie} {emp.Nazwisko}");
                ClientComboBox1.ItemsSource = clientsList.Select(emp => $"{emp.Firma}");
            }
            else
            {
                MessageBox.Show("Nie udało się połączyć z bazą danych. Sprawdź ustawienia połączenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            this.DataContext = this;
            Items = new ObservableCollection<Osoba>();
            SearchData();
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = Dostep.DataSource,
                    InitialCatalog = Dostep.InitialCatalog,
                    IntegratedSecurity = Dostep.IntegratedSecurity,
                    UserID = Dostep.UserID,
                    Password = Dostep.Password,
                    ConnectTimeout = Dostep.ConnectTimeout,
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    string sqlQuery = "SELECT k.Imie AS ImieKlienta, " +
                        "k.Nazwisko AS NazwiskoKlienta, " +
                        "k.Firma AS FirmaKlienta, " +
                        "p.Nazwisko AS NazwiskoPracownika, " +
                        "u.Opis AS OpisUslugi, " +
                        "CAST(u.Data AS DATE) AS DataUslugi, " +
                        "u.Czas AS CzasUslugi, " +
                        "u.Sprzet AS SprzetUslugi, " +
                        "u.Dojazd AS DojazdUslugi FROM  " +
                        "u.ID AS IDUslugi FROM " +
                        "Uslugi u JOIN Klienci k ON u.ID_Klienta = k.ID JOIN Pracownicy p ON u.ID_Pracownika = p.ID;";

                    DataAccess.LoadItemsFromDatabase(sqlQuery, Items, pom);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void SearchData()
        {
            DateTime startDate, endDate;
            string selectedFirma,  selectedNazwisko;
            if (pom == true)
            {
                selectedFirma = ClientComboBox2.SelectedItem as string;
                selectedNazwisko = EmployeeComboBox2.SelectedItem as string;
                startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
                endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;
            }
            else
            {
                selectedFirma = ClientComboBox3.SelectedItem as string;
                selectedNazwisko = EmployeeComboBox3.SelectedItem as string;
                startDate = StartDatePicker2.SelectedDate ?? DateTime.MinValue;
                endDate = EndDatePicker2.SelectedDate ?? DateTime.MaxValue;
            }
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = Dostep.DataSource,
                    InitialCatalog = Dostep.InitialCatalog,
                    UserID = Dostep.UserID,
                    Password = Dostep.Password,
                    ConnectTimeout = Dostep.ConnectTimeout,
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    string sqlQuery = "SELECT k.Imie AS ImieKlienta, k.Nazwisko AS NazwiskoKlienta, k.Firma AS FirmaKlienta, p.Nazwisko AS NazwiskoPracownika, u.Opis AS OpisUslugi, u.Data AS DataUslugi, u.Czas AS CzasUslugi,\r\n    u.Sprzet AS SprzetUslugi,\r\n    u.Dojazd AS DojazdUslugi\r\n, u.ID AS IDUslugi\r\nFROM \r\n    Uslugi u\r\nJOIN \r\n    Klienci k ON u.ID_Klienta = k.ID\r\nJOIN \r\n    Pracownicy p ON u.ID_Pracownika = p.ID\r\nWHERE 1=1";

                    sqlQuery += $" AND u.Data >= '{startDate:yyyy-MM-dd}' AND u.Data <= '{endDate:yyyy-MM-dd}'";

                    if (!string.IsNullOrEmpty(selectedFirma))
                    {
                        sqlQuery += $" AND k.Firma LIKE '{selectedFirma}'";
                    }

                    if (!string.IsNullOrEmpty(selectedNazwisko))
                    {
                        sqlQuery += $" AND p.Nazwisko LIKE '{selectedNazwisko}'";
                    }

                    DataAccess.LoadItemsFromDatabase(sqlQuery, Items, pom);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public void LoadFirmyFromDatabase()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = Dostep.DataSource,
                    InitialCatalog = Dostep.InitialCatalog,
                    UserID = Dostep.UserID,
                    Password = Dostep.Password,
                    ConnectTimeout = Dostep.ConnectTimeout
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT Firma FROM Klienci";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ClientComboBox2.Items.Clear();
                        ClientComboBox3.Items.Clear();

                        ClientComboBox2.Items.Add("");
                        ClientComboBox3.Items.Add("");

                        while (reader.Read())
                        {
                            string firma = reader.GetString(0);
                            ClientComboBox2.Items.Add(firma);
                            ClientComboBox3.Items.Add(firma);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas ładowania firm: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadNazwiskaFromDatabase()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = Dostep.DataSource,
                    InitialCatalog = Dostep.InitialCatalog,
                    UserID = Dostep.UserID,
                    Password = Dostep.Password,
                    ConnectTimeout = Dostep.ConnectTimeout,
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT Nazwisko FROM Pracownicy";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        EmployeeComboBox2.Items.Clear();
                        EmployeeComboBox3.Items.Clear();

                        EmployeeComboBox2.Items.Add("");
                        EmployeeComboBox3.Items.Add("");

                        while (reader.Read())
                        {
                            string nazwisko = reader.GetString(0);
                            EmployeeComboBox2.Items.Add(nazwisko);
                            EmployeeComboBox3.Items.Add(nazwisko);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania nazwisk z bazy danych: " + ex.Message);
            }
        }
        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate, endDate;
            string selectedFirma, selectedNazwisko;
            if (pom == true)
            {
                selectedFirma = ClientComboBox2.SelectedItem as string;
                selectedNazwisko = EmployeeComboBox2.SelectedItem as string;
                startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
                endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;
            }else{
                selectedFirma = ClientComboBox3.SelectedItem as string;
                selectedNazwisko = EmployeeComboBox3.SelectedItem as string;
                startDate = StartDatePicker2.SelectedDate ?? DateTime.MinValue;
                endDate = EndDatePicker2.SelectedDate ?? DateTime.MaxValue;
            }
            
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = Dostep.DataSource,
                    InitialCatalog = Dostep.InitialCatalog,
                    UserID = Dostep.UserID,
                    Password = Dostep.Password,
                    ConnectTimeout = Dostep.ConnectTimeout,
                };
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    string sqlQuery = "SELECT k.Imie AS ImieKlienta, k.Nazwisko AS NazwiskoKlienta, k.Firma AS FirmaKlienta, p.Nazwisko AS NazwiskoPracownika, u.Opis AS OpisUslugi, u.Data AS DataUslugi, u.Czas AS CzasUslugi,\r\n    u.Sprzet AS SprzetUslugi,\r\n    u.Dojazd AS DojazdUslugi\r\n, u.ID as ID FROM \r\n    Uslugi u\r\nJOIN \r\n    Klienci k ON u.ID_Klienta = k.ID\r\nJOIN \r\n    Pracownicy p ON u.ID_Pracownika = p.ID\r\nWHERE 1=1";
                    sqlQuery += $" AND u.Data >= '{startDate:yyyy-MM-dd}' AND u.Data <= '{endDate:yyyy-MM-dd}'";

                    if (!string.IsNullOrEmpty(selectedFirma))
                    {
                        sqlQuery += $" AND k.Firma LIKE '{selectedFirma}'";
                    }

                    if (!string.IsNullOrEmpty(selectedNazwisko))
                    {
                        sqlQuery += $" AND p.Nazwisko LIKE '{selectedNazwisko}'";
                    }
                    
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Items.Clear();
                           
                            while (reader.Read())
                            {
                                Osoba osoba = new Osoba
                                {
                                    IsChecked = pom,
                                    Imie = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                    Nazwisko = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Firma = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Pracownik = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Opis = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    Data = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                                    Czas = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                                    Sprzet = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                    Dojazd = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                    ID = reader.IsDBNull(9) ? 0 : reader.GetInt32(9)
                                };
                                osoba.CzasString = osoba.Czas.ToString() + " h";
                                if (osoba.Dojazd == (decimal)-1.00)
                                {
                                    osoba.DojazdString = "zdalnie";
                                }
                                else
                                {
                                    osoba.DojazdString = osoba.Dojazd.ToString() + " km";
                                }

                                Items.Add(osoba);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool generateDoc = GenerateDocCheckBox.IsChecked ?? false;
                bool generatePDF = GeneratePDFCheckBox.IsChecked ?? false;
                if (!generateDoc && !generatePDF)
                {
                    MessageBox.Show("Proszę zaznaczyć co najmniej jedną opcję generacji raportu (.exe lub .pdf).", "Ostrzeżenie");
                    return;
                }
                string programPath = AppDomain.CurrentDomain.BaseDirectory;
                string templatePath = Path.Combine(programPath, "Raport.doc");
                string tempTemplatePath = Path.GetTempFileName();
                System.IO.File.Copy(templatePath, tempTemplatePath, true);
                Application wordApp = new Application();
                Document doc = wordApp.Documents.Open(tempTemplatePath);

                Assembly assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(templatePath))
                {
                    if (stream != null)
                    {
                        string tempPath = Path.GetTempFileName();

                        using (FileStream fileStream = System.IO.File.Create(tempPath))
                        {
                            stream.CopyTo(fileStream);
                        }

                        Application wordApplication = new Application();
                        Document document = wordApplication.Documents.Open(tempPath);

                    }
                    else
                    {
                        Console.WriteLine("Nie można znaleźć zasobu.");
                    }
                }

                Table table = doc.Tables.Add(doc.Range(doc.Content.End - 1), 1, 7);

                table.PreferredWidthType = WdPreferredWidthType.wdPreferredWidthAuto;

                string[] headers = { "Firma klienta", "Nazwisko pracownika", "Opis usługi", "Data usługi", "Czas usługi", "Sprzęt", "Dojazd" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Cell(1, i + 1).Range.Text = headers[i];
                    table.Cell(1, i + 1).Range.Font.Name = "Times New Roman";
                    table.Cell(1, i + 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(1, i + 1).Range.ParagraphFormat.SpaceBefore = 2;
                    table.Cell(1, i + 1).Range.ParagraphFormat.SpaceAfter = 2;
                    if (headers[i] == "Data usługi" || headers[i] == "Czas usługi" || headers[i] == "Dojazd")
                    {
                        table.Columns[i + 1].SetWidth(65, WdRulerStyle.wdAdjustNone);
                    }
                    if (headers[i] == "Opis usługi")
                    {
                        table.Columns[i + 1].SetWidth(200, WdRulerStyle.wdAdjustNone);
                    }
                }

                int currentRow = 2;

                foreach (Osoba osoba in Items)
                {
                    if (osoba.IsChecked)
                    {
                        table.Rows.Add();
                        table.Cell(currentRow, 1).Range.Text = osoba.Firma;
                        table.Cell(currentRow, 2).Range.Text = osoba.Pracownik;
                        table.Cell(currentRow, 3).Range.Text = osoba.Opis;
                        table.Cell(currentRow, 4).Range.Text = osoba.Data.ToShortDateString();
                        table.Cell(currentRow, 5).Range.Text = osoba.CzasString;
                        table.Cell(currentRow, 6).Range.Text = osoba.Sprzet;
                        table.Cell(currentRow, 7).Range.Text = osoba.DojazdString;

                        for (int i = 1; i <= 7; i++)
                        {
                            table.Cell(currentRow, i).Range.Font.Name = "Times New Roman";
                            table.Cell(currentRow, i).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                        currentRow++;
                    }
                }
                decimal totalHours = 0;
                decimal totalKilometers = 0;

                foreach (Osoba osoba in Items)
                {
                    if (osoba.IsChecked)
                    {
                        totalHours += osoba.Czas;
                        if (osoba.Dojazd != -1)
                        {
                            totalKilometers += osoba.Dojazd;
                        }
                    }
                }

                table.Rows.Add();
                table.Cell(currentRow, 4).Range.Text = "Suma Czas:";
                table.Cell(currentRow, 5).Range.Text = totalHours.ToString() + " h";
                table.Cell(currentRow, 5).Range.Font.Name = "Times New Roman";
                table.Cell(currentRow, 5).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                table.Cell(currentRow, 7).Range.Text = totalKilometers.ToString() + " km";
                table.Cell(currentRow, 7).Range.Font.Name = "Times New Roman";
                table.Cell(currentRow, 7).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                for (int i = 1; i <= 3; i++)
                {
                    Borders borders = table.Cell(currentRow, i).Borders;
                    borders[WdBorderType.wdBorderTop].LineWidth = 0;
                    borders[WdBorderType.wdBorderLeft].LineWidth = 0;
                    borders[WdBorderType.wdBorderRight].LineWidth = 0;
                    borders[WdBorderType.wdBorderBottom].LineWidth = 0;
                }

                table.Cell(currentRow, 6).Range.Text = "Suma Dojazd";
                table.Cell(currentRow, 6).Range.Font.Name = "Times New Roman";
                table.Cell(currentRow, 6).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                for (int i = 4; i <= 7; i++)
                {
                    table.Cell(currentRow, i).Range.Font.Name = "Times New Roman";
                    table.Cell(currentRow, i).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                }

                currentRow++;

                table.Borders.Enable = 1;
                Cell cell1 = table.Cell(currentRow, 1);
                Cell cell2 = table.Cell(currentRow, 2);
                Cell cell3 = table.Cell(currentRow, 3);

                cell1.Borders.Enable = (int)WdLineStyle.wdLineStyleNone;
                cell2.Borders.Enable = (int)WdLineStyle.wdLineStyleNone;
                cell3.Borders.Enable = (int)WdLineStyle.wdLineStyleNone;

                cell1.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell2.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell3.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;

                cell3.Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Dokument Word (*.doc)|*.doc";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    if (generateDoc && !generatePDF)
                    {
                        string wordFilePath = Path.ChangeExtension(filePath, ".doc");
                        doc.SaveAs2(wordFilePath);
                    }
                    else if (generatePDF && !generateDoc)
                    {
                        string pdfFilePath = Path.ChangeExtension(filePath, ".pdf");
                        doc.ExportAsFixedFormat(pdfFilePath, WdExportFormat.wdExportFormatPDF);
                        Process.Start(pdfFilePath); 
                    }
                    else if (generateDoc && generatePDF)
                    {
                        string wordFilePath = Path.ChangeExtension(filePath, ".doc");
                        doc.SaveAs2(wordFilePath);

                        string pdfFilePath = Path.ChangeExtension(filePath, ".pdf");
                        doc.ExportAsFixedFormat(pdfFilePath, WdExportFormat.wdExportFormatPDF);
                        Process.Start(pdfFilePath);
                    }

                    doc.Close();
                    Marshal.ReleaseComObject(doc);

                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);

                    SearchData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd COM podczas generowania raportu: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", "Błąd");
            }
        }
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedEmployeeName = EmployeeComboBox1.SelectedItem as string;
                string selectedClientName = ClientComboBox1.SelectedItem as string;
                DateTime date = DatePicker.SelectedDate ?? DateTime.Now;
                string description = DescriptionTextBox.Text;
                string equipment = EquipmentTextBox.Text;
                int travelDistance;
                float time;

                string timeText = TimeTextBox.Text.Replace('.', ',');

                if (string.IsNullOrWhiteSpace(selectedEmployeeName) || string.IsNullOrWhiteSpace(selectedClientName) || date == null)
                {
                    MessageBox.Show("Wprowadź wymagane dane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Czy na pewno chcesz dodać to zamówienie?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string connectionString = Dostep.GetConnectionString();

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        int selectedEmployeeID = employeesList.FirstOrDefault(emp => $"{emp.Imie} {emp.Nazwisko}" == selectedEmployeeName)?.ID ?? -1;

                        int selectedClientID = clientsList.FirstOrDefault(cli => $"{cli.Firma}" == selectedClientName)?.ID ?? -1;

                        float.TryParse(timeText, out time);

                        if (CheckBox.IsChecked == true)
                        {
                            travelDistance = -1;
                        }
                        else
                        {
                            if (!int.TryParse(TravelDistanceTextBox.Text, out travelDistance))
                            {
                                travelDistance = 0;
                            }
                        }

                        string query = @"INSERT INTO Uslugi (ID_Pracownika, ID_Klienta, Data, Czas, Opis, Sprzet, Dojazd)
                VALUES (@EmployeeID, @ClientID, @Date, @Time, @Description, @Equipment, @TravelDistance)";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", selectedEmployeeID);
                            command.Parameters.AddWithValue("@ClientID", selectedClientID);
                            command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                            command.Parameters.AddWithValue("@Time", time);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Equipment", equipment);
                            command.Parameters.AddWithValue("@TravelDistance", travelDistance);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Zamówienie zostało dodane do bazy danych.", "Sukces", MessageBoxButton.OK);
                                Refresh_Click(null, null);
                            }
                            else
                            {
                                MessageBox.Show("Nie udało się dodać zamówienia do bazy danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania zamówienia do bazy danych: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TravelDistanceTextBox.Text = "Zdalnie";
            TravelDistanceTextBox.IsEnabled = false;
        }
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            TravelDistanceTextBox.Text = "";
            TravelDistanceTextBox.IsEnabled = true;
        }
        private void AddNewClient_Click(object sender, RoutedEventArgs e)
        {
            Window2 window2 = new Window2();
            window2.Show();
        }
        public void AddClientToDatabase(string imie, string nazwisko, string firma)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = Dostep.DataSource,
                InitialCatalog = Dostep.InitialCatalog,
                UserID = Dostep.UserID,
                Password = Dostep.Password,
                ConnectTimeout = Dostep.ConnectTimeout
            };

            string connectionString = builder.ConnectionString;
            string query = "INSERT INTO Klienci (Imie, Nazwisko, Firma) VALUES (@Imie, @Nazwisko, @Firma)";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Imie", imie);
                        command.Parameters.AddWithValue("@Nazwisko", nazwisko);
                        command.Parameters.AddWithValue("@Firma", firma);
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Klient został dodany pomyślnie!", "Sukces", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } 
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            RotateButton(sender as Button, 60);
        }
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            RotateButton(sender as Button, 0);
        }
        private void RotateButton(Button button, double targetAngle)
        {
            if (button != null)
            {
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = button.RenderTransform is RotateTransform existingTransform ? existingTransform.Angle : 0;
                animation.To = targetAngle;
                animation.Duration = TimeSpan.FromSeconds(0.2);

                if (button.RenderTransform is RotateTransform transform)
                {
                    transform.BeginAnimation(RotateTransform.AngleProperty, animation);
                }
                else
                {
                    RotateTransform newTransform = new RotateTransform();
                    button.RenderTransform = newTransform;
                    button.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    newTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
                }
            }
        }
        public void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (grid1.Visibility == Visibility.Visible)
            {
                TimeTextBox.Text = "";
                DescriptionTextBox.Text = "";
                EquipmentTextBox.Text = "";
                TravelDistanceTextBox.Text = "";
                EmployeeComboBox1.SelectedIndex = -1;
                ClientComboBox1.SelectedIndex = -1;
                DatePicker.SelectedDate = null;
                CheckBox.IsChecked = false;

                clientsList = dbOperations.LoadClientsFromDatabase();
                employeesList = dbOperations.LoadEmployeesFromDatabase();
                EmployeeComboBox1.ItemsSource = employeesList.Select(emp => $"{emp.Imie} {emp.Nazwisko}");
                ClientComboBox1.ItemsSource = clientsList.Select(emp => $"{emp.Firma}");
                LoadFirmyFromDatabase();
                LoadNazwiskaFromDatabase();
            }
            else if (grid2.Visibility == Visibility.Visible)
            {
                StartDatePicker.SelectedDate = null;
                EndDatePicker.SelectedDate = null;
                ClientComboBox2.SelectedIndex = -1;
                EmployeeComboBox2.SelectedIndex = -1;

                SearchButton_Click(sender, e);
                SearchData();

                LoadFirmyFromDatabase();
                LoadNazwiskaFromDatabase();
            }
            else if (grid3.Visibility == Visibility.Visible)
            {
                StartDatePicker2.SelectedDate = null;
                EndDatePicker2.SelectedDate = null;
                ClientComboBox3.SelectedIndex = -1;
                EmployeeComboBox3.SelectedIndex = -1;

                SearchButton_Click(sender, e);
                SearchData();

                LoadFirmyFromDatabase();
                LoadNazwiskaFromDatabase();
            }
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditFun editFun = new EditFun();
            editFun.Edit_Click(sender, e, mainWindow);
        }

        // --- MENU --- //
        private void MenuConnection_Click(object sender, RoutedEventArgs e)
        {
            Window1 settingsWindow = new Window1();
            if (settingsWindow.ShowDialog() == true)
            {
                string serverName = settingsWindow.ServerName;
                string databaseName = settingsWindow.DatabaseName;
                string userName = settingsWindow.UserName;
                string password = settingsWindow.Password;

                string connectionString = $"Data Source={serverName};Initial Catalog={databaseName};User ID={userName};Password={password};";
            }
        }
        private void MenuInsert_Click(object sender, RoutedEventArgs e)
        {
            grid2.Visibility = Visibility.Hidden;
            grid1.Visibility = Visibility.Visible;
            grid3.Visibility = Visibility.Hidden;
            Title = "Dodaj zamówienie";
        }
        private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        {
            grid1.Visibility = Visibility.Hidden;
            grid2.Visibility = Visibility.Visible;
            grid3.Visibility = Visibility.Hidden;
            Title = "Generuj raport";
            pom = true;
        }
        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            grid1.Visibility = Visibility.Hidden;
            grid2.Visibility = Visibility.Hidden;
            grid3.Visibility = Visibility.Visible;
            Title = "Edytuj rekordy";
            pom = false;
        }
        // --- ---- --- //
    }
}
