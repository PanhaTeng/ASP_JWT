using ASP_JWT.Models;
using ASP_JWT.Models.Dto;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
namespace ASP_JWT.Data
{
    public class MapperProfile:Profile
    {
        public MapperProfile()
        {
            /*CreateMap<CustomerDto, Customer>();*/
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<Course,CourseCreate>().ReverseMap();
            CreateMap<Course,CourseUpdate>().ReverseMap();
        }
    }
}
