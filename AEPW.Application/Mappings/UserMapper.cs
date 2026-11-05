//using AEPW.Domain.DTOs;
//using AEPW.Domain.Entities;

//namespace AEPW.Application.Mappings;

///// <summary>
///// Manual mapping helpers — no AutoMapper dependency.
///// Mapping lives in Application so domain entities never carry UI concerns.
///// </summary>
//public static class UserMapper
//{
//    public static UserDto ToDto(User entity) => new()
//    {
//        Id          = entity.Id,
//        FirstName   = entity.FirstName,
//        LastName    = entity.LastName,
//        Email       = entity.Email,
//        PhoneNumber = entity.PhoneNumber,
//        Status      = entity.Status,
//        CreatedAt   = entity.CreatedAt,
//        UpdatedAt   = entity.UpdatedAt
//    };
//}
