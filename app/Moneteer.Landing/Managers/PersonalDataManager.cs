using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Models;

namespace Moneteer.Landing.Managers
{
    public class PersonalDataManager : IPersonalDataManager
    {
        private readonly ApiClientHelper _apiClientHelper;

        public PersonalDataManager(ApiClientHelper apiClientHelper)
        {
            _apiClientHelper = apiClientHelper;
        }

        public async Task<PersonalData> GetPersonalDataAsync(User user)
        {
            var personalData = new PersonalData();

            personalData.UserData = GetUserData(user);

            var appData = await GetAppData();

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

        private async Task<int> GetAppData()
        {
            var token = await _apiClientHelper.GetAccessToken();
            var client = _apiClientHelper.GetClient(token);

            var response = await client.GetAsync("healthcheck");

            return 0;

        }
    }
}
