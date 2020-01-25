using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleNotes.Api.Data;

namespace SimpleNotes.Api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationContext>();

            services.Configure<IdentityOptions>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });


            services.AddSession(options => {
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationContext context) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            app.UseSession();

            // Create database
            context.Database.Migrate();
            string createTable = "CREATE TABLE IF NOT EXISTS ";
            string table = "Notes (Id TEXT PRIMARY KEY, Title TEXT, Content TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "Labels (Id TEXT PRIMARY KEY, Name TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "UserNotes (UserId TEXT, NoteId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "UserLabels (UserId TEXT, LabelId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "NoteLabels (NoteId TEXT, LabelId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
        }
    }
}
