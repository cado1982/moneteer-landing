﻿namespace Moneteer.Landing.Sql
{
    public static class SubscriptionSql
    {
        public static string GetStripeId = @"SELECT stripe_id FROM identity.users WHERE id = @UserId";
        public static string SetStripeId = @"UPDATE identity.users SET stripe_id = @StripeId WHERE id = @UserId";
        public static string UpdateExpiry = @"UPDATE identity.users SET subscription_expiry = @Expiry WHERE stripe_id = @StripeId";
        public static string UpdateStatus = @"UPDATE identity.users SET subscription_status = @Status WHERE stripe_id = @StripeId";
        public static string GetUserId = @"SELECT id FROM identity.users WHERE stripe_id = @StripeId";
        public static string GetSubscriptionInfo = @"
            SELECT 
                stripe_id as CustomerId,
                subscription_id as Id,
                subscription_expiry as SubscriptionExpiry,
                trial_expiry as TrialExpiry,
                subscription_status as Status
            FROM 
                identity.users
            WHERE
                id = @UserId";
    }
}
