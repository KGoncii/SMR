using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SMR3
{
    public class DatabaseOperations
    {
        private string connectionString;

        public DatabaseOperations(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Employee> LoadEmployeesFromDatabase()
        {
            List<Employee> employeesList = new List<Employee>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Imie, Nazwisko, ID FROM Pracownicy";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employee employee = new Employee
                            {
                                Imie = reader.GetString(0),
                                Nazwisko = reader.GetString(1),
                                ID = reader.GetInt32(2)
                            };
                            employeesList.Add(employee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa wyjątków
                Console.WriteLine("Błąd podczas wczytywania pracowników z bazy danych: " + ex.Message);
            }

            return employeesList;
        }

        public List<Client> LoadClientsFromDatabase()
        {
            List<Client> clientsList = new List<Client>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Nazwisko, Firma, ID FROM Klienci ORDER BY Firma";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Client client = new Client
                            {
                                Nazwisko = reader.GetString(0),
                                Firma = reader.GetString(1),
                                ID = reader.GetInt32(2)
                            };
                            clientsList.Add(client);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa wyjątków
                Console.WriteLine("Błąd podczas wczytywania klientów z bazy danych: " + ex.Message);
            }

            return clientsList;
        }
    }
}
