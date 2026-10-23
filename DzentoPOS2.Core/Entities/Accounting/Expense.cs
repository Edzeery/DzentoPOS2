using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Accounting
{
    public class Expense : AuditableEntity
    {
        public required string Title { get; set; }

        public Guid ExpenseCategoryId { get; set; }

        public Guid BranchId { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string? Notes { get; set; }
    }
}