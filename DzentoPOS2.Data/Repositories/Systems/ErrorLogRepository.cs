using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Systems.Interfaces;

namespace DzentoPOS2.Data.Repositories.Systems
{
    public class ErrorLogRepository
        : Repository<ErrorLog>,
          IErrorLogRepository
    {
        public ErrorLogRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}