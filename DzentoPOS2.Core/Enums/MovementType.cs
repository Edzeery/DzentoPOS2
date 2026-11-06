using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DzentoPOS2.Core.Enums
{
    public enum MovementType
    {
        Purchase = 1,
        Sale = 2,
        SaleReturn = 3,
        PurchaseReturn = 4,
        StockAdjustmentIncrease = 5,
        StockAdjustmentDecrease = 6,
        StockTransferIn = 7,
        StockTransferOut = 8,
        SaleOut = 9,
        PurchaseIn = 10,
        SaleVoidIn = 11,
        Opening = 12,
        SaleCashIn = 13,
        SaleVoidCashOut = 14,
        SessionPayIn = 15,
        SessionPayOut = 16
    }
}