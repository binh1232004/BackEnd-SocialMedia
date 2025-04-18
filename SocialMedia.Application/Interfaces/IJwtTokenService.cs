using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);

    }
}
