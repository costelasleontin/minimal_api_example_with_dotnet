using Microsoft.EntityFrameworkCore; // UseSqlServer
using Microsoft.Extensions.DependencyInjection; // IServiceCollection

namespace Packt.Shared;

public static class NorthwindContextExtensions
{
    /// <summary>
    /// Adds NorthwindContext to the specified IServiceCollection. Uses the SqlServer database provider.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">Set to override the default.</param>
    /// <returns>An IServiceCollection that can be used to add moreservices.</returns>
    public static IServiceCollection AddNorthwindContext(
    this IServiceCollection services,
    //This is implemented directlly for simplycity purposes. We would use app settings to to store connection string and we would have to store secrets in other secure places.
    string connectionString = "Data Source=.;Initial Catalog=Northwind;Integrated Security=False;TrustServerCertificate=True;User Id=PlaceholderForUserName;Password=ThisIsNotTheActualPassword:P!")
    {
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.LogTo(Console.WriteLine, new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting
        });
        });
        return services;
    }
}