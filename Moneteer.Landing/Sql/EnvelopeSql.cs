using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Sql
{
    public static class EnvelopeSql
    {
        public static string CreateEnvelopeCategory = @"
            INSERT INTO envelope_category (id, budget_id, name, is_deleted, is_hidden) VALUES (@Id, @BudgetId, @Name, false, false);";

        public static string CreateEnvelope = @"
            INSERT INTO 
                envelope (
                    id,
                    envelope_category_id,
                    name,
                    is_deleted,
                    is_hidden,
                    balance) 
                VALUES (
                    @Id,
                    @EnvelopeCategoryId,
                    @Name,
                    false,
                    false,
                    0);";
    }
}
