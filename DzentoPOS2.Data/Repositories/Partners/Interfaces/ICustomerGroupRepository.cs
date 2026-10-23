using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Partners.Interfaces
{
    public interface ICustomerGroupRepository : IRepository<CustomerGroup>
    {
        Task<IEnumerable<CustomerGroup>> GetActiveGroupsAsync();
    }
}