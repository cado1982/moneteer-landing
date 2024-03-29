﻿namespace Moneteer.Landing.Sql
{
    public static class SubscriptionSql
    {
        public static string SetStripeId = @"UPDATE identity.users SET stripe_id = @StripeId WHERE id = @UserId";
        public static string UpdateExpiry = @"UPDATE identity.users SET subscription_expiry = @Expiry WHERE stripe_id = @StripeId";
        public static string UpdateStatus = @"UPDATE identity.users SET subscription_status = @Status WHERE stripe_id = @StripeId";
        public static string Update = @"UPDATE identity.users SET subscription_status = @Status, subscription_id = @SubscriptionId WHERE stripe_id = @StripeId";
    }
}
