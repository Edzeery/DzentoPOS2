using System;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Systems
{
    public interface IBranchAware
    {
        Task OnBranchChangedAsync(Guid branchId);
    }
}
