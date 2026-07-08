using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AppFW.Utilities.RuleEngine
{
    public static class RuleEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddRuleEngineExecution(
            this IServiceCollection services,
            Action<RuleEngineOptions> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);

            // Register the execution service as a singleton
            services.TryAddSingleton<IRuleEngineExecutionService>(sp =>
            {
                // Resolve options
                var options = sp.GetRequiredService<IOptions<RuleEngineOptions>>().Value;

                // Validate required options
                if (string.IsNullOrWhiteSpace(options.RulesPath))
                {
                    throw new RulesConfigurationException("RuleEngineOptions.RulesPath is required.");
                }

                // Create and return the execution service
                var svc = new RuleEngineExecutionService(options);
                return svc;
            });

            return services;
        }
    }
}

