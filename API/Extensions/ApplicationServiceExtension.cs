using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddAplicationService(
            this IServiceCollection services, // specify the things we are extending using this keyword
            IConfiguration config
        )
        {
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("SqlConnection"));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();

            return services; 
        }
    }
}
