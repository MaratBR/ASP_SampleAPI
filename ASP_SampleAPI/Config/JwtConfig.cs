using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_SampleAPI.Config
{
    public class JwtConfig
    {
        public string Issuer { get; set; }

        public TimeSpan TokenLifeTime { get; set; } = TimeSpan.FromMinutes(45);

        public TimeSpan RefreshTokenLifeTime { get; set; } = TimeSpan.FromDays(14);

        public string SecretKey { get; set; }

        public string Audience { get; set; }

        internal SecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(SecretKey));
        }
    }
}
