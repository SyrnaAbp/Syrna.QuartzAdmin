using AutoMapper;
using Syrna.QuartzAdmin.ExecutionHistory;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;

namespace Syrna.QuartzAdmin
{
    public class QuartzAdminApplicationAutoMapperProfile : Profile
    {
        public QuartzAdminApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<QuartzExecutionHistory, ExecutionLogDto>();
            CreateMap<ExecutionHistoryDetail, ExecutionLogDetailDto>();
        }
    }
}
