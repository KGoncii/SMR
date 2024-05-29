using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace SMR3
{
    public class DataAccess
    {
        public static void LoadItemsFromDatabase(string sqlQuery, ObservableCollection<Osoba> items, bool pom)
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

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        items.Clear();

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
                            osoba.DojazdString = osoba.Dojazd == (decimal)-1.00 ? "zdalnie" : osoba.Dojazd.ToString() + " km";

                            items.Add(osoba);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
