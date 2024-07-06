using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NOS.Engineering.Challenge.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
            services.AddSingleton<MongoDBContext>();
            services.AddScoped<IContentsManager, MongoContentsManager>();
            
            services.AddControllers();
        }
    }
}
