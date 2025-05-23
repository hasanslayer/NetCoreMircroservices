﻿using AspnetRunBasics.Extensions;
using AspnetRunBasics.Models;

namespace AspnetRunBasics.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(HttpClient httpClient, ILogger<CatalogService> logger)
        {
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalog()
        {
            _logger.LogDebug($"get catalog products from {_client.BaseAddress}");

            var response = await _client.GetAsync("/Catalog");
            return await response.ReadContentAs<List<CatalogModel>>();
        }

        public async Task<CatalogModel> GetCatalog(string id)
        {
            var response = await _client.GetAsync($"/Catalog/{id}");
            return await response.ReadContentAs<CatalogModel>();
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalogByCateogory(string category)
        {
            var response = await _client.GetAsync($"/Catalog/GetProductByCateogry/{category}");
            return await response.ReadContentAs<List<CatalogModel>>();
        }

        public async Task<CatalogModel> CreateCatalog(CatalogModel model)
        {
            var response = await _client.PostAsJson($"/Catalog", model);
            if (response.IsSuccessStatusCode)
            {
                return await response.ReadContentAs<CatalogModel>();
            }
            else
            {
                throw new Exception("Something went wrong when calling api.");
            }
        }
    }
}
