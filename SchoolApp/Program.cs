using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SchoolApp.Configuration;
using SchoolApp.Data;
using SchoolApp.Helpers;
using SchoolApp.Middleware;
using SchoolApp.Repositories;
using SchoolApp.Security;
using SchoolApp.Services;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace SchoolApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connString = builder.Configuration.GetConnectionString("DevConnection");
            builder.Services.AddDbContext<SchoolRest10Context>(options => options.UseSqlServer(connString));

            builder.Host.UseSerilog((hostingContext, configuration) =>
            {
                configuration.ReadFrom.Configuration(hostingContext.Configuration);
            });

            builder.Services.AddRepositories();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddSingleton<IEncryptionUtil,  EncryptionUtil>();
            builder.Services.AddSingleton<IActivationTokenUtil, ActivationTokenUtil>();

            builder.Services.AddScoped<DbSeeder>();                                       // DbSeeder added

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MapperConfig>());

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;   // how Kestrel recognizes the user. It reads the JWT from the header,the Authorization: Bearer<token>.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;        // what the Kestrel does when the user is unauthenticated - sends 401 Unauthorized message.
            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = builder.Environment.IsDevelopment();    // with this we set the response packet to include error description for debugging. (In production level is false by default).
                options.TokenValidationParameters = new TokenValidationParameters     
                {                                                                   // with options.SaveToken = true, if save of the raw token at HttpContext is required, so that it can be obtained with: HttpContext.GetTokenAsync"access_token"). 
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
                };
            });
                                                                                           // CORS policy configuration
            builder.Services.AddCors(options =>                 // Note also that UseCors() MUST be PRIOR to UseAuthentication() as browser sends a preflight request(Options) first, in order to ask whether the access is permitted or not.
            {                                                      // If CORS middleware is not running first, the the preflight will be rejected by the authentication middleware( not havind JWT token) and the browser will never send the real request!!! 
                options.AddPolicy("AllowCFClient", policy => policy.WithOrigins(builder.Configuration["Cors:Origin"]!)
                .AllowAnyMethod()
                .AllowAnyHeader());
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });


            builder.Services.AddEndpointsApiExplorer();    // scan all the endpoints and make a description of them.
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "School App", Version = "v1" });     // Always having endpoints WITH versioning. As in here! SOS!!!
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authentication header using the Bearer Scheme",
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    });
                options.OperationFilter<AuthorizeOperationFilter>();
            });


            // Add services to the container.

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("VIEW_USERS", p => p.RequireClaim("Capability", "VIEW_USERS"));
                options.AddPolicy("INSERT_TEACHER", p => p.RequireClaim("Capability", "INSERT_TEACHER"));
                options.AddPolicy("INSERT_STUDENT", p => p.RequireClaim("Capability", "INSERT_STUDENT"));
                options.AddPolicy("VIEW_TEACHERS", p => p.RequireClaim("Capability", "VIEW_TEACHERS"));
                options.AddPolicy("EDIT_TEACHER", p => p.RequireClaim("Capability", "EDIT_TEACHER"));
                options.AddPolicy("DELETE_TEACHER", p => p.RequireClaim("Capability", "DELETE_TEACHER"));
                options.AddPolicy("VIEW_STUDENTS", p => p.RequireClaim("Capability", "VIEW_STUDENTS"));
                options.AddPolicy("EDIT_STUDENT", p => p.RequireClaim("Capability", "EDIT_STUDENT"));
                options.AddPolicy("DELETE_STUDENT", p => p.RequireClaim("Capability", "DELETE_STUDENT"));
                options.AddPolicy("INSERT_COURSE", p => p.RequireClaim("Capability", "INSERT_COURSE"));
                options.AddPolicy("VIEW_COURSES", p => p.RequireClaim("Capability", "VIEW_COURSES"));
                options.AddPolicy("VIEW_COURSE", p => p.RequireClaim("Capability", "VIEW_COURSE"));
                options.AddPolicy("EDIT_COURSE", p => p.RequireClaim("Capability", "EDIT_COURSE"));
                options.AddPolicy("DELETE_COURSE", p => p.RequireClaim("Capability", "DELETE_COURSE"));
            });

            builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrCapabilityHandler>();

            // END of Service registration area (above). Container configs. No object has beed created yet!!!!


            var app = builder.Build();             // container is being built - below onwards


            // START of the section where the container has already been built and we can ask  objects from app.Services. (eg: Request pipeline, middleware config, seeding and startup actions).    

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                await seeder.SeedAsync();
            }


                app.UseExceptionHandler();              // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "School App v1"));  // If we had v1 , v2 , v3 etc we must state them as this. and upper in the code too (in line 79).
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "School App v2"));
            }

                                                                 // In this order below! SOS!!!!
            app.UseHttpsRedirection();

            app.UseCors("AllowCFClient");
            app.UseAuthentication();                            // Enables the authentication middleware in the pipeline. The incoming requests will be checked for JWT token and the HttpContext.User will becoming populate. The HttpContext.User is the principal.
            app.UseMiddleware<MustChangePasswordMiddleware>();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
