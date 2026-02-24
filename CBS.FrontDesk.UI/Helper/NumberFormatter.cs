using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class NumberFormatter
    {
        public static decimal ToDecimalPlaces(decimal value, int decimalPlaces = 1)
        {
            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }
    }
}