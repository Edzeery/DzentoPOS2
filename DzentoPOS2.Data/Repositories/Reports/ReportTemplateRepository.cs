using DzentoPOS2.Core.Entities.Reports;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Reports.Interfaces;

namespace DzentoPOS2.Data.Repositories.Reports
{
    public class ReportTemplateRepository
        : Repository<ReportTemplate>,
          IReportTemplateRepository
    {
        public ReportTemplateRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}