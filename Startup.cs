using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace test_space
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // JWT Authentication
            services.AddAuthentication(
                // JwtBearerDefaults.AuthenticationScheme
            )
                .AddJwtBearer(options => {
                    var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
                    options.TokenValidationParameters = new TokenValidationParameters() {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["Authentication:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = Configuration["Authentication:Audience"],

                        ValidateLifetime = true,

                        IssuerSigningKey = new SymmetricSecurityKey(secretByte)
                    };
                    // this allow you to access token inside controller thru HttpContext.GetTokenAsync("access_token")
                    options.SaveToken = true;
                })
                .AddApiKeySupport(options => {});


            services.AddControllers()
            // 將API回傳之JSON格式與API Model一致
            .AddNewtonsoftJson(options => {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "test_space", Version = "v1" });
            });

            
            // CORS Setting
            services.AddCors(options => {
                options.AddDefaultPolicy(policy => {
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // User Define Service
            services.AddTransient<IGetApiKeyQuery, InMemoryGetApiKeyQuery>();

            services.AddAuthorization(options =>
                {                    
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        JwtBearerDefaults.AuthenticationScheme,
                        ApiKeyAuthenticationOptions.DefaultScheme
                    );
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                    options.AddPolicy(Policies.OnlyEmployees, policy => {
                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults.AuthenticationScheme,
                            ApiKeyAuthenticationOptions.DefaultScheme
                        );
                        policy.Requirements.Add(new OnlyEmployeesRequirement());
                    });
                    options.AddPolicy(Policies.OnlyManagers, policy => {
                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults.AuthenticationScheme,
                            ApiKeyAuthenticationOptions.DefaultScheme
                        );
                        policy.Requirements.Add(new OnlyManagersRequirement());
                    });
                    options.AddPolicy(Policies.OnlyThirdParties, policy => {
                        policy.AddAuthenticationSchemes(
                            JwtBearerDefaults.AuthenticationScheme,
                            ApiKeyAuthenticationOptions.DefaultScheme
                        );
                        policy.Requirements.Add(new OnlyThirdPartyRequirement());
                    });
                }
            );
            services.AddSingleton<IAuthorizationHandler, OnlyEmployeesAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, OnlyManagersAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, OnlyThirdPartyAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "test_space v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
