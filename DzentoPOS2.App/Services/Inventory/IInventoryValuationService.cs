using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Inventory
{
    public interface IInventoryValuationService
    {
        Task<InventoryValuationDto> GetValuationAsync(CancellationToken cancellationToken = default);

        void InvalidateCache();
    }
}