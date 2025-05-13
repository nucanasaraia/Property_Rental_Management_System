using AutoMapper;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.DTOs.PaymentDTOs;
using PropertyRentalManagementSystem.DTOs.PropertiesDTOs;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.Requests;

namespace PropertyRentalManagementSystem.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Property, PropertyListDto>();
            CreateMap<Property, PropertyDetailDto>();
            CreateMap<AddProperty, Property>();
            CreateMap<Rental, RentalDto>();
            CreateMap<Contract, ContractDto>();
            CreateMap<MaintenanceTicket, MaintenanceDTO>();
            CreateMap<AddTicket, MaintenanceTicket>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<Payment, PaymentsDto>();
            CreateMap<PaymentsDto, Payment>();
        }
    }
}
