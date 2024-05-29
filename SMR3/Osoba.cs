using System;

namespace SMR3
{
    public class Osoba
    {
        public bool IsChecked { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Firma { get; set; }
        public string Pracownik { get; set; }
        public string Opis { get; set; }
        public DateTime Data { get; set; }
        public decimal Czas { get; set; }
        public string Sprzet { get; set; }
        public decimal Dojazd { get; set; }
        public string CzasString { get; set; }
        public string DojazdString { get; set; }
        public int? ID { get; set; }
        public Osoba()
        {
            Data = DateTime.MinValue;
        }
    }
}
