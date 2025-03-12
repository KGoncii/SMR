using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace SMR3
{
    internal class Dostep
    {
        public static string DataSource { get; private set; } // Nazwa komputera
        public static string InitialCatalog { get; private set; } // Nazwa bazy danych 
        public static bool IntegratedSecurity { get; } = false;
        public static string UserID { get; private set; } // Nazwa użytkownika
        public static string Password { get; private set; } // Hasło użytkownika
        public static int ConnectTimeout { get; } = 5; // Czas przez który program będzie próbował się połączyć

        static Dostep()
        {
            LoadCredentialsFromFile();
        }

        private static void LoadCredentialsFromFile()
        {
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(programPath, "Dostep.txt");

            if (File.Exists(filePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    DataSource = GetValueFromLine(lines[0]);
                    InitialCatalog = GetValueFromLine(lines[1]);
                    UserID = GetValueFromLine(lines[2]);
                    Password = GetValueFromLine(lines[3]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd odczytu pliku Dostep.txt: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Plik Dostep.txt nie istnieje. Ustawiono wartości domyślne.");
            }
        }

        private static string GetValueFromLine(string line)
        {
            // Linie w pliku powinny mieć format "Nazwa: Wartość"
            string[] parts = line.Split(':');
            if (parts.Length == 2)
            {
                return parts[1].Trim();
            }
            else
            {
                throw new FormatException("Nieprawidłowy format linii w pliku Dostep.txt.");
            }
        }

        public static string GetConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = DataSource,
                InitialCatalog = InitialCatalog,
                IntegratedSecurity = IntegratedSecurity,
                UserID = UserID,
                Password = Password,
                ConnectTimeout = ConnectTimeout
            };
            return builder.ConnectionString;
        }
    }
}
