using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Models;
using System;
using System.Threading.Tasks;

namespace Moneteer.Landing.Managers
{
    public interface IPersonalDataManager
    {
        Task<PersonalData> GetPersonalDataAsync(User userId, string accessToken);
    }
}
