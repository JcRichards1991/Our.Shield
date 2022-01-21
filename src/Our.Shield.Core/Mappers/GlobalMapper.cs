using AutoMapper;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Our.Shield.Core.Mappers
{
    /// <summary>
    /// Global Profile for Shield's Mappings
    /// </summary>
    public class GlobalProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GlobalProfile"/>
        /// </summary>
        public GlobalProfile()
        {
            CreateMap<Data.Dtos.Environment, Models.Environment>()
                .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<Models.Domain>>(src.Domains)));

            CreateMap<Models.Environment, Data.Dtos.Environment>()
                .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Domains)));
        }
    }
}
