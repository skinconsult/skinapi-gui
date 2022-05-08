﻿using System.Net.Http;
using System.Threading.Tasks;

namespace SkinApi.Gui.Clients
{
    public interface ISkinApiClient
    {
        public Task<IEnumerable<CompanyRecord>> GetCompaniesAsync();
        public void AddCompanyAsync(CompanyRecord inputModel);
        public void UpdateCompaniesAsync(IEnumerable<CompanyRecord> companies);
    }
}