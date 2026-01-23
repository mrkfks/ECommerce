using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using Nest;

namespace ECommerce.Infrastructure.Services.Search
{
    public class ElasticsearchProductSearchService : ISearchService
    {
        private readonly IElasticClient _elasticClient;
        private const string IndexName = "products";

        public ElasticsearchProductSearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<bool> IndexProductAsync(ProductDto product)
        {
            var response = await _elasticClient.IndexAsync(product, idx => idx.Index(IndexName));
            return response.IsValid;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var response = await _elasticClient.DeleteAsync<ProductDto>(productId, d => d.Index(IndexName));
            return response.IsValid;
        }

        public async Task<PaginatedResult<ProductDto>> SearchProductsAsync(string query, int page, int pageSize, int? categoryId = null)
        {
            var searchRequest = new SearchRequest(IndexName)
            {
                From = (page - 1) * pageSize,
                Size = pageSize,
                Query = new BoolQuery
                {
                    Must = new List<QueryContainer>
                    {
                        new QueryStringQuery { Fields = new[] { "name", "description" }, Query = query }
                    },
                    Filter = categoryId.HasValue ? new List<QueryContainer> { new TermQuery { Field = "categoryId", Value = categoryId.Value } } : null
                }
            };
            var response = await _elasticClient.SearchAsync<ProductDto>(searchRequest);
            var items = response.Documents.ToList();
            var total = (int)response.Total;
            return new PaginatedResult<ProductDto>(items, total, page, pageSize);
        }
    }
}
