using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Models;
using Moneteer.Backend.Client;

namespace Moneteer.Landing.Managers
{
    public class PersonalDataManager : IPersonalDataManager
    {
        private readonly MoneteerApiClient _apiClient;

        public PersonalDataManager(MoneteerApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PersonalData> GetPersonalDataAsync(User user, string accessToken)
        {
            var personalData = new PersonalData();

            personalData.UserData = GetUserData(user);
            personalData.AppData = await GetAppData(accessToken);

            return personalData;
        }

        private Dictionary<string, string> GetUserData(User user)
        {
            var userData = new Dictionary<string, string>();
            var userDataProps = typeof(User).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in userDataProps)
            {
                userData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            return userData;
        }

        private async Task<AppData> GetAppData(string accessToken)
        {
            var result = new AppData();

            var budgets = await _apiClient.GetBudgetsAsync(accessToken);
            var budget = budgets.FirstOrDefault();
            if (budget == null) return null;

            result.Transactions = await _apiClient.GetTransactionsAsync(budget.Id, accessToken);
            result.Accounts = await _apiClient.GetAccountsAsync(budget.Id, accessToken);
            result.EnvelopeCategories = await _apiClient.GetEnvelopeCategoriesAsync(budget.Id, accessToken);
            result.Envelopes = await _apiClient.GetEnvelopesAsync(budget.Id, accessToken);
            
            return result;
        }
    }
}
