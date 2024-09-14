using Mamlaka.API.Interfaces;
using Mamlaka.API.RedisORM;
using Mamlaka.API.Repositories;
using Redis.OM;

namespace Mamlaka.API.Services.HostedService;
public class HostedService : IHostedService
    {
        private IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly RedisConnectionProvider _provider;
        public HostedService(
            RedisConnectionProvider provider,
            IServiceProvider serviceProvider,
            IConfiguration configuration
            )
        {
            _provider = provider;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// check if the indeces exist, if not create them
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string _server = _configuration["Server:backendServer"];

            IEnumerable<string> info = (await _provider.Connection.ExecuteAsync("FT._LIST")).ToArray().Select(x => x.ToString());

            if (info.All(x => x != "transactionredismodel-idx"))
            {
                await _provider.Connection.CreateIndexAsync(typeof(TransactionRedisModel));
            }            

            if (_server == "prod")
            {
                //this is how we wrap a scopped injected service to avoid exceptions
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    ITransactionRepository _repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

                    await _repo.LoadTransactionsToRedisCache();
                   
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
