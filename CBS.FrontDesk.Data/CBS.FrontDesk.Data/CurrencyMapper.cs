using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.API.Helper
{
    public static class CurrencyMapper
    {
        public static CurrencyNotes MapToCurrencyNotesRequest(List<CurrencyNotesDto> currencyNotesList)
        {
            var currencyNotesRequest = new CurrencyNotes();

            foreach (var currencyNote in currencyNotesList)
            {
                if (currencyNote.DinominationType == "Note")
                {
                    switch (currencyNote.Denomination)
                    {
                        case "10000":
                            currencyNotesRequest.note10000 += currencyNote.Value;
                            break;
                        case "5000":
                            currencyNotesRequest.note5000 += currencyNote.Value;
                            break;
                        case "2000":
                            currencyNotesRequest.note2000 += currencyNote.Value;
                            break;
                        case "1000":
                            currencyNotesRequest.note1000 += currencyNote.Value;
                            break;
                        case "500":
                            currencyNotesRequest.note500 += currencyNote.Value;
                            break;
                    }
                }
                else if (currencyNote.DinominationType == "Coin")
                {
                    switch (currencyNote.Denomination)
                    {
                        case "500":
                            currencyNotesRequest.coin500 += currencyNote.Value;
                            break;
                        case "100":
                            currencyNotesRequest.coin100 += currencyNote.Value;
                            break;
                        case "50":
                            currencyNotesRequest.coin50 += currencyNote.Value;
                            break;
                        case "25":
                            currencyNotesRequest.coin25 += currencyNote.Value;
                            break;
                        case "10":
                            currencyNotesRequest.coin10 += currencyNote.Value;
                            break;
                        case "5":
                            currencyNotesRequest.coin5 += currencyNote.Value;
                            break;
                        case "1":
                            currencyNotesRequest.coin1 += currencyNote.Value;
                            break;
                    }
                }
            }

            return currencyNotesRequest;
        }
    }
}
