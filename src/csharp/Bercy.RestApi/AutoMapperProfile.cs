﻿namespace Bercy.RestApi
{
    using AutoMapper;
    using Front.Shared;
    using Slices;

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Slice, SliceDto>();
            CreateMap<TaxHouseholdCompositionDto, TaxHouseholdComposition>();
            CreateMap<TaxComputationRequestDto, TaxComputationRequest>();
            CreateMap<Tax, TaxDto> ();
        }
    }
}
