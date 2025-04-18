using Microsoft.EntityFrameworkCore;
using MoviesMinimalAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MoviesMinimalAPI.DTOs;
using MoviesMinimalAPI.Services;
using MoviesMinimalAPI.MoviesMappers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MoviesMinimalAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net;
using FluentValidation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MoviesMinimalAPI.Classes;


var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<HashService>();
builder.Services.AddTransient<IFileManagerService, FileManagerService>();
builder.Services.AddHttpContextAccessor();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MoviesMapper));

// Configure DB connection
builder.Services.AddDbContext<MoviesMinimalAPIDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add authentication 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"])),
                   RoleClaimType = ClaimTypes.Role
               });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("Admin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    options.AddPolicy("User", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("User"));
});

// Setting up security in Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "JWT Authentication Using Bearer Scheme. \r\n\r " +
        "Enter the word 'Bearer' followed by a space and the authentication token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
                        new string[]{}
                    }
                });
    c.OperationFilter<ImageFileOperationFilter>();

});

//CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Cache
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();


// Categories endpoints
var categories = app.MapGroup("/api/categories").WithTags("Categories");

categories.MapGet("/", [ResponseCache(Duration = 20)]  async (ICategoryService categoryService) =>
{
    var listCategoriesDTO = await categoryService.GetAllAsyncService();
    return Results.Ok(listCategoriesDTO);
})
.AllowAnonymous()
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status403Forbidden);

categories.MapGet("/{categoryId:int}", [ResponseCache(Duration = 20)] async (int categoryId, ICategoryService categoryService) =>
{
    var itemCategoryDTO = await categoryService.GetByIdAsyncService(categoryId);

    if (itemCategoryDTO == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(itemCategoryDTO);
})
.CacheOutput("20Seconds")
.AllowAnonymous()
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status403Forbidden)
.WithName("GetCategoryById");

categories.MapPost("/", async (CategoryCreateDTO categoryCreateDTO, ICategoryService categoryService) =>
{
    if (categoryCreateDTO == null)
    {
        return Results.BadRequest();
    }

    if (string.IsNullOrEmpty(categoryCreateDTO.Name) || await categoryService.ExistsByNameAsyncService(categoryCreateDTO.Name))
    {
        return Results.Problem(
            detail: "Category already exists or name is null",
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    var categoryDTO = await categoryService.CreateAsyncService(categoryCreateDTO);

    if (categoryDTO == null)
    {
        return Results.Problem(
            detail: $"Something went wrong while saving the record {categoryCreateDTO.Name}",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    return Results.CreatedAtRoute(
        "GetCategoryById",
        new { categoryId = categoryDTO.IdCategory },
        categoryDTO
    );
})
.RequireAuthorization("Admin")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status500InternalServerError);

categories.MapPut("/{categoryId:int}", async (int categoryId, CategoryUpdateDTO categoryUpdateDTO, ICategoryService categoryService) =>
{
    if (categoryUpdateDTO == null || categoryId != categoryUpdateDTO.IdCategory)
    {
        return Results.BadRequest();
    }

    try
    {
        await categoryService.UpdateAsyncService(categoryId, categoryUpdateDTO);
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Something went wrong updating the registry: {ex.Message}",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
})
.WithName("UpdateCategory")
.RequireAuthorization("Admin")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status500InternalServerError);

categories.MapDelete("/{categoryId:int}", async (int categoryId, ICategoryService categoryService) =>
{
    if (!await categoryService.ExistsByIdAsyncService(categoryId))
    {
        return Results.NotFound($"Category with ID {categoryId} not found.");
    }

    if (!await categoryService.DeleteAsyncService(categoryId))
    {
        return Results.Problem(
            detail: $"Something went wrong deleting the record with id {categoryId}",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    return Results.NoContent();
})
.WithName("DeleteCategory")
.RequireAuthorization("Admin")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status500InternalServerError);

// Movies endpoints
var movies = app.MapGroup("/api/movies").WithTags("Movies")
    .RequireAuthorization("Admin"); // Default authorization for the group

movies.MapGet("/", [ResponseCache(Duration = 20)] async (IMovieService movieService) =>
{
    try
    {
        var movies = await movieService.GetAllAsyncService();
        return Results.Ok(new APIResponses
        {
            Success = true,
            Result = movies,
            StateCode = HttpStatusCode.OK
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapGet("/{movieId:int}", [ResponseCache(Duration = 20)]  async (int movieId, IMovieService movieService) =>
{
    try
    {
        var movie = await movieService.GetByIdAsyncService(movieId);
        if (movie == null)
        {
            return Results.NotFound(new APIResponses
            {
                Success = false,
                StateCode = HttpStatusCode.NotFound,
                Errors = new List<string> { "Movie not found" }
            });
        }
        return Results.Ok(new APIResponses
        {
            Success = true,
            Result = movie,
            StateCode = HttpStatusCode.OK
        });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.NotFound,
            Errors = new List<string> { ex.Message }
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.WithName("GetMovieById")
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapGet("/GetMoviesByCategory/{categoryId:int}", [ResponseCache(Duration = 20)]  async (int categoryId, IMovieService movieService) =>
{
    try
    {
        var movies = await movieService.GetByCategoryAsyncService(categoryId);
        if (movies == null || !movies.Any())
        {
            return Results.NotFound(new APIResponses
            {
                Success = false,
                StateCode = HttpStatusCode.NotFound,
                Errors = new List<string> { "No movies found for this category" }
            });
        }
        return Results.Ok(new APIResponses
        {
            Success = true,
            Result = movies,
            StateCode = HttpStatusCode.OK
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapGet("/SearchMovieByName", [ResponseCache(Duration = 20)]  async (string name, IMovieService movieService) =>
{
    try
    {
        var movies = await movieService.SearchByNameAsyncService(name);
        if (movies == null || !movies.Any())
        {
            return Results.NotFound(new APIResponses
            {
                Success = false,
                StateCode = HttpStatusCode.NotFound,
                Errors = new List<string> { "No movies found with that name" }
            });
        }
        return Results.Ok(new APIResponses
        {
            Success = true,
            Result = movies,
            StateCode = HttpStatusCode.OK
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapPost("/", async ([FromForm] MovieCreateDTO movieCreateDTO,
    IMovieService movieService,
    [FromServices] IValidator<MovieCreateDTO> validator) =>
{
    var validationResult = await validator.ValidateAsync(movieCreateDTO);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        });
    }
    try
    {
        var createdMovie = await movieService.CreateAsyncService(movieCreateDTO);
        return Results.CreatedAtRoute(
            "GetMovieById",
            new { movieId = createdMovie.IdMovie },
            new APIResponses
            {
                Success = true,
                Result = createdMovie,
                StateCode = HttpStatusCode.Created
            });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.Conflict,
            Errors = new List<string> { ex.Message }
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
}).DisableAntiforgery()
.Produces<APIResponses>(StatusCodes.Status201Created)
.Produces<APIResponses>(StatusCodes.Status400BadRequest)
.Produces<APIResponses>(StatusCodes.Status409Conflict)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapPut("/{movieId:int}", async (int movieId, [FromForm] MovieUpdateDTO movieUpdateDTO,
    IMovieService movieService,
    [FromServices] IValidator<MovieUpdateDTO> validator) =>
{
    var validationResult = await validator.ValidateAsync(movieUpdateDTO);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        });
    }

    try
    {
        await movieService.UpdateAsyncService(movieId, movieUpdateDTO);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = new List<string> { ex.Message }
        });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.NotFound,
            Errors = new List<string> { ex.Message }
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.DisableAntiforgery()
.Produces(StatusCodes.Status204NoContent)
.Produces<APIResponses>(StatusCodes.Status400BadRequest)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

movies.MapDelete("/{movieId:int}", async (int movieId, IMovieService movieService) =>
{
    try
    {
        await movieService.DeleteAsyncService(movieId);
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.NotFound,
            Errors = new List<string> { ex.Message }
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.Produces(StatusCodes.Status204NoContent)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

// Users endpoints
var users = app.MapGroup("/api/users").WithTags("Users")
    .RequireAuthorization("Admin"); // Default authorization for the group

users.MapGet("/", [ResponseCache(Duration = 20)]  async ([FromServices] IUserService userService) =>
{
    try
    {
        var users = await userService.GetUserService();
        return Results.Ok(new APIResponses
        {
            Success = true,
            Result = users,
            StateCode = HttpStatusCode.OK
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

users.MapDelete("/{email}", async (string email, [FromServices] IUserService userService) =>
{
    try
    {
        var result = await userService.DeleteUserService(email);
        if (result == null)
        {
            return Results.NotFound(new APIResponses
            {
                Success = false,
                StateCode = HttpStatusCode.NotFound,
                Errors = new List<string> { "User not found" }
            });
        }

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
})
.Produces(StatusCodes.Status204NoContent)
.Produces<APIResponses>(StatusCodes.Status404NotFound)
.Produces<APIResponses>(StatusCodes.Status500InternalServerError);

// Auth endpoints
var auth = app.MapGroup("/api/auth").WithTags("Auth");

// Register endpoint
auth.MapPost("/register", async ([FromBody] UserRegistrationDTO userRegistrationDTO,
    [FromServices] IUserService userService,
    [FromServices] IValidator<UserRegistrationDTO> validator) =>
{
    var validationResult = await validator.ValidateAsync(userRegistrationDTO);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        });
    }

    userRegistrationDTO.Role = string.IsNullOrWhiteSpace(userRegistrationDTO.Role) ||
        userRegistrationDTO.Role.Trim().ToLower() == "string"
            ? "User"
            : userRegistrationDTO.Role.Trim();

    var allowedRoles = new List<string> { "User", "Admin" };
    if (!allowedRoles.Contains(userRegistrationDTO.Role))
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = new List<string> { "Invalid role" }
        });
    }

    var userDTO = await userService.AddUserService(userRegistrationDTO);
    if (userDTO == null)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = new List<string> { "Failed to create user" }
        });
    }

    return Results.Ok(new APIResponses
    {
        Success = true,
        Result = userDTO,
        StateCode = HttpStatusCode.OK
    });
})
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status400BadRequest);

// Login endpoint
auth.MapPost("/login", async ([FromBody] UserLoginDTO user,
    [FromServices] IUserService userService,
    [FromServices] ITokenService tokenService,
    [FromServices] IValidator<UserLoginDTO> validator) =>
{
    var validationResult = await validator.ValidateAsync(user);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        });
    }

    var userDB = await userService.GetByEmailUserService(user.Email);

    if (userDB == null)
    {
        return Results.Unauthorized();
    }

    bool isValidPassword = userService.VerifyPasswordUserService(user.Password, userDB);
    if (!isValidPassword)
    {
        return Results.Unauthorized();
    }

    var tokenResponse = tokenService.GenerateTokenService(user);
    return Results.Ok(new APIResponses
    {
        Success = true,
        Result = tokenResponse,
        StateCode = HttpStatusCode.OK
    });
})
.AllowAnonymous()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

// Change password endpoint
auth.MapPost("/changePassword", async ([FromBody] UserChangePasswordDTO userChangePasswordDTO,
    [FromServices] IUserService userService,
    [FromServices] IValidator < UserChangePasswordDTO > validator) =>

{
    var validationResult = await validator.ValidateAsync(userChangePasswordDTO);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        });
    }
    bool result = await userService.ChangePasswordUserService(
        userChangePasswordDTO.Email,
        userChangePasswordDTO.OldPassword,
        userChangePasswordDTO.NewPassword);

    if (!result)
    {
        return Results.BadRequest(new APIResponses
        {
            Success = false,
            StateCode = HttpStatusCode.BadRequest,
            Errors = new List<string> { "Invalid credentials or user not found" }
        });
    }

    return Results.Ok(new APIResponses
    {
        Success = true,
        Result = "Password changed successfully",
        StateCode = HttpStatusCode.OK
    });
})
.RequireAuthorization()
.Produces<APIResponses>(StatusCodes.Status200OK)
.Produces<APIResponses>(StatusCodes.Status400BadRequest);

app.Run();
