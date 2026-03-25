using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class OrganizationApiClient : BaseApiClient, IOrganizationApiClient
    {
        public OrganizationApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<OrganizationDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<OrganizationDTO>>("organization", cancellationToken) ?? Array.Empty<OrganizationDTO>();
        }

        public async Task<OrganizationDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<OrganizationDTO>($"organization/{id}", cancellationToken);
        }
    }
}
