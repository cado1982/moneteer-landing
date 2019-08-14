using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Sql
{
    public static class BudgetSql
    {
        public static string Create = @"
            INSERT INTO 
                app.budget (id, 
                        name, 
                        user_id,
                        available,
                        currency_code,
                        thousands_separator,
                        decimal_separator,
                        decimal_places,
                        currency_symbol_location,
                        date_format)
            VALUES 
                (@Id, 
                 @Name, 
                 @UserId, 
                 0,
                 @CurrencyCode, 
                 @ThousandsSeparator, 
                 @DecimalSeparator, 
                 @DecimalPlaces, 
                 @CurrencySymbolLocation, 
                 @DateFormat);";
    }
}
