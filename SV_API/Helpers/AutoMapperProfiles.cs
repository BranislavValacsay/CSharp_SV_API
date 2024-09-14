using AutoMapper;
using sp_api.DTO;
using sp_api.Models;

namespace sp_api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap <RequestServerDTO, RequestServer>();
            CreateMap <RequestServer, RequestServerDTO>()
                .ForMember(dest => dest.NetworkDTO, opt => opt.MapFrom(src => src.VMMNetwork));
            CreateMap<PUT_RequestServer_DTO,RequestServer>();

            CreateMap <VMMNetwork, VMMNetworkDTO>();
            CreateMap <VMMNetworkDTO, VMMNetwork>();

            CreateMap <ServerNameDto, RequestServer>();

            CreateMap <ServerIpAddressDto, RequestServer>();

            CreateMap <VMMServer,VmmServerDTO>()
                .ForMember(x => x.Location, opt => opt.MapFrom(a => a.Location.Name));

            CreateMap <WindowsVersion,WindowsVersionDTO>();

        }
    }
}
