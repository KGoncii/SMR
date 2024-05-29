using System;
using System.IO;
using System.Windows;


namespace SMR3
{
    public partial class Window1 : Window
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }


        public Window1()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            this.DataContext = this;
            FillTextBoxesWithCredentials();
        }
        private void FillTextBoxesWithCredentials()
        {
            ServerNameTextBox.Text = Dostep.DataSource;
            DatabaseNameTextBox.Text = Dostep.InitialCatalog;
            UserNameTextBox.Text = Dostep.UserID;
            PasswordTextBox.Text = Dostep.Password;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ServerName = ServerNameTextBox.Text;
            DatabaseName = DatabaseNameTextBox.Text;
            UserName = UserNameTextBox.Text;
            Password = PasswordTextBox.Text;

            SaveCredentialsToFile();

            DialogResult = true;
            MessageBox.Show("Reset programu");
            Close();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveCredentialsToFile()
        {
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(programPath, "Dostep.txt");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Nazwa serwera: {ServerName}");
                writer.WriteLine($"Nazwa bazy danych: {DatabaseName}");
                writer.WriteLine($"Nazwa użytkownika: {UserName}");
                writer.WriteLine($"Hasło: {Password}");
            }
        }
    }
}
