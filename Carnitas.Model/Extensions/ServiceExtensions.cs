using Microsoft.Extensions.DependencyInjection;

namespace Carnitas.Model.Extensions;

public static class ServiceExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddDatabase()
        {
            serviceCollection.AddScoped<OperationRepository>();
            
            return serviceCollection;
        }
    }
}