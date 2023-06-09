﻿using AutoMapper;
using {{RootNamespace}}.DTO;
using {{RootNamespace}}.Entities;
using {{RootNamespace}}.Model;

namespace {{RootNamespace}}.Mapping;

[ExcludeFromCodeCoverage]
public class PeopleProfile : Profile
{
    public PeopleProfile()
    {
        CreateMap<Person, PersonDto>();
        CreateMap<PersonDto, Person>();
        
        CreateMap<PersonCreateRequestModel, Person>();
        CreateMap<PersonUpdateRequestModel, Person>();
        
        CreateMap<Person, PersonResponseModel>();
        CreateMap<PersonDto, PersonResponseModel>();
    }
}
