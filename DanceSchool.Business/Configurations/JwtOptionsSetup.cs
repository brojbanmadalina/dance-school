namespace DanceSchool.Business.Configurations
{
    using DanceSchool.Business.Models.Auth;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
    {
        private readonly IConfiguration _config;

        public JwtOptionsSetup(IConfiguration config)
        {
            _config = config;
        }

        public void Configure(JwtOptions options)
        {
            _config.GetSection("AuthSettings:Jwt").Bind(options);
        }
    }
}
