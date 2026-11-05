using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Services.Barcodes
{
    public interface IBarcodeScanService
    {
        Task<string?> ScanOnceAsync(IWin32Window owner, CancellationToken ct = default);
        bool IsEnabled { get; }
    }
}