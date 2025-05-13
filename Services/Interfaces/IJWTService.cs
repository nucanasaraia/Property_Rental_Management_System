using PropertyRentalManagementSystem.CORE;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.Services.Interfaces
{
    public interface IJWTService
    {
        UserToken GetUserToken(User user);
    }
}
