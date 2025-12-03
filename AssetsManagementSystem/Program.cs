using AssetsManagementSystem.Services.Report;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace AssetsManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            #region Add services to the container

            builder.Services.AddCors(options =>
           options.AddDefaultPolicy(builder =>
           builder
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowAnyOrigin()
          )
                           );
            builder.Services.AddDataProtection();

            builder.Services.AddOthersServices(builder.Configuration);

            builder.Services.AddDataLayer(builder.Configuration);

            builder.Services.AddFolderOfServices();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
           
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ReportService> ( );
            // ...
            builder.Services.AddScoped<AssetsManagementSystem.Services.Printing.PrintingService> ( );
            // ...
            builder.Services.AddControllers()
               .AddJsonOptions(config =>
                   config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            builder.Services.AddHttpClient ( );
            #endregion

            #region Swagger Configuration

            builder.Services.AddSwaggerGen(c =>
            {
                c.UseInlineDefinitionsForEnums();
                c.SchemaFilter<EnumSchemaFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Asset Management System API", Version = "v1", Description = "AMS API swagger client." });

                // Adding Bearer token authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "'You can type 'Bearer' and enter the token after leaving a space \r\n\r\n For example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

            });

            #endregion

            var app = builder.Build();

            using ( var scope = app.Services.CreateScope ( ) )
            {
                var services = scope.ServiceProvider;
                try
                {
                    // ?????? ??? ??????? ????????? ???? ??? ??????
                    await AssetsManagementSystem.DbSeeder.SeedAsync ( services );
                }
                catch ( Exception ex )
                {
                    // ?? ??? ????? ???? ????? Seed ??????? ?? ?????? ???? ???? ?????
                    var logger = services.GetRequiredService<ILogger<Program>> ( );
                    logger.LogError ( ex, "An error occurred while seeding the database." );
                }
            }

            // Configure the HTTP request pipeline.
            if ( app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Management System API v1");
                    //c.RoutePrefix = string.Empty; // To access Swagger directly from the root
                });
            }
            using ( var serviceScope = app.Services.CreateScope ( ) )
            {
                var services = serviceScope.ServiceProvider;
                await DbSeeder.SeedAsync ( services );
            }


            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors();
 
            app.MapControllers();

            app.ExceptionHandleConfiguration();

            app.Run();
        }
    }
}
