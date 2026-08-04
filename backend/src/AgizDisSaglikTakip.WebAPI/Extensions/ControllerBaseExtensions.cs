using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Extensions;

public static class ControllerBaseExtensions
{
    public static int GetUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(JwtRegisteredClaimNames.Sub);
        return int.Parse(claim!.Value);
    }
}
