using AutoMapper;

namespace UserRewards.API.Profiles
{
    public class UserRewardsCoreProfile : Profile
    {
        public UserRewardsCoreProfile()
        {
            CreateMap<Core.Models.Domain.Transaction, Core.Models.DTO.Transaction>();
            CreateMap<Core.Models.DTO.Transaction, Core.Models.Domain.Transaction>()
                .ForMember(t => t.RemainingPoints, options => options.MapFrom(t => t.Points < 0 ? 0 : t.Points))
                .ForMember(t => t.Payer, options => options.MapFrom(t => t.Payer.ToUpper()))
                .ForMember(t => t.Timestamp, options => options.MapFrom(t => t.Timestamp.ToUniversalTime()));
        }
    }
}
