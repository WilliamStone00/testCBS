using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class Currency
    {
        // ISO 4217 currency code
        public string  Code { get; set; }

        // Full currency name
        public string  Name { get; set; }

        // Number of decimal places
        public int DecimalPlaces { get; set; }

        // Symbol used to represent currency    
        public string  Symbol { get; set; }

        // Current exchange rate to base currency
        public decimal  Rate { get; set; }

        // Format as string using currency symbol
        public string ToString(decimal amount)
        {
            return $"{ Symbol}{amount}:{DecimalPlaces}";
        }

        public List<Currency> CreateCurrencies()
        {
            var currencies = new List<Currency>()
    {
        new Currency() {
            Name = "US Dollar",
             Code = "USD",
            Symbol = "$",
            Rate = 1.0M
        },
        new Currency() {
            Name = "Euro",
            Code = "EUR",
            Symbol = "€",
            Rate = 0.89M
        },
        new Currency() {
            Name = "Indian Rupee",
            Code = "INR",
            Symbol = "₹",
            Rate = 0.013M
        },
        new Currency() {
            Name = "British Pound",
            Code = "GBP",
            Symbol = "£",
            Rate = 1.30M
        },new Currency() {
            Name = "Central African CFA Franc",
            Code = "XAF",
            Symbol = "FCFA",
            Rate = 0.0016M
        }, new Currency() {
            Name = "Japanese Yen",
            Code = "JPY",
            Symbol = "¥",
            Rate = 0.0074M
        },
        new Currency() {
             Name = "Canadian Dollar",
             Code = "CAD",
             Symbol = "CA$",
             Rate = 0.73M
        },
        new Currency() {
             Name = "Australian Dollar",
            Code = "AUD",
            Symbol = "A$",
             Rate = 0.69M
        },
        new Currency() {
            Name = "Chinese Yuan Renminbi",
            Code = "CNY",
            Symbol = "¥",
            Rate = 0.14M
        },
         new Currency() {
            Name = "Swedish Krona",
            Code = "SEK",
            Symbol = "kr",
            Rate = 0.093M
        }
        // Add more currencies
    };
             return currencies;
            // Use currencies list here    
        }
    }
}
