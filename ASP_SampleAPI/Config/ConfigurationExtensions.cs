using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_SampleAPI.Config
{
    public static class ConfigurationExtensions
    {
        public static JwtConfig GetJwtConfig(this IConfiguration configuration) => configuration.GetSection("JwtConfig").Get<JwtConfig>();
    }
}
