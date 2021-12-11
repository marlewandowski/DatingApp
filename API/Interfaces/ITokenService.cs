using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.Extensions.Configuration;

namespace API.Interfaces
{
    public interface ITokenService
    {
        string createtoken(AppUser user);
    }
}