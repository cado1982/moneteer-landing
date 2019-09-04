namespace Moneteer.Landing.Sql
{
    public static class SubscriptionSql
    {
        public static string GetStripeId = @"SELECT stripe_id FROM identity.users WHERE id = @UserId";
        public static string SetStripeId = @"UPDATE identity.users SET stripe_id = @StripeId WHERE id = @UserId";
    }
}
