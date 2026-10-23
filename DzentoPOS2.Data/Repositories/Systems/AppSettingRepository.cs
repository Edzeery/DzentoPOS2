using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Systems.Interfaces;

namespace DzentoPOS2.Data.Repositories.Systems
{
    public class AppSettingRepository
        : Repository<AppSetting>,
          IAppSettingRepository
    {
        public AppSettingRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}