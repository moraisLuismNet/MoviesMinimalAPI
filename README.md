## MoviesMinimalAPI
ASP.NET Core Web API MoviesAPI


![MoviesMinimalAPI](img/1.png)
![MoviesMinimalAPI](img/2.png)


## Program
```cs
builder.Services.AddDbContext<MoviesMinimalAPIDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"))
);
``` 

## appsetting.Development.json
```cs
{
  "ConnectionStrings": {
        "Connection": "Server=*;Database=MoviesMinimalAPI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
``` 

[DeepWiki moraisLuismNet/MoviesMinimalAPI](https://deepwiki.com/moraisLuismNet/MoviesMinimalAPI)