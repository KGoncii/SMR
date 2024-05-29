using System.Windows;


namespace SMR3
{
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            this.DataContext = this;
        }
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            string imie = ImieTextBox.Text;
            string nazwisko = NazwiskoTextBox.Text;
            string firma = FirmaTextBox.Text;

            if (firma.Length > 18)
            {
                MessageBox.Show("Podana nazwa firmy jest dłuższa niż 18 znaków co może prowadzić do jej błędnego wyświetlania w podglądzie\n(Na raporcie dalej będzie wyświetlać się poprawnie).\nRozważ jej skrócenie", "Ostrzeżenie");
            }

            if (string.IsNullOrWhiteSpace(firma))
            {
                MessageBox.Show("Nazwa firmy jest wymagana.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Czy na pewno chcesz dodać klienta: Imię: {imie}, Nazwisko: {nazwisko}, Firma: {firma}?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

                if (mainWindow != null)
                {
                    mainWindow.AddClientToDatabase(imie, nazwisko, firma);
                }
                else
                {
                    MessageBox.Show("Nie udało się uzyskać instancji głównego okna.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Close();

                MainWindow mainWindow1 = Application.Current.MainWindow as MainWindow;
                if (mainWindow1 != null)
                {   
                    if (mainWindow1.grid3.Visibility == Visibility.Hidden)
                    mainWindow1.Refresh_Click(null, null);
                }
                else
                {
                    MessageBox.Show("Nie udało się uzyskać instancji głównego okna.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            this.Close();  
        }
    }
}
