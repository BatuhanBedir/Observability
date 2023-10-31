using StackExchange.Redis;

namespace Order.API.RedisServices;

public class RedisService
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    public RedisService(IConfiguration configuration)
    {
        var host = configuration.GetSection("Redis")["Host"];
        var port = configuration.GetSection("Redis")["Port"];

        var config = $"{host}:{port}";

        _connectionMultiplexer = ConnectionMultiplexer.Connect(config);
    }
    public IDatabase GetDb(int db)
    {
        return _connectionMultiplexer.GetDatabase(db);
    }
    public ConnectionMultiplexer GetConnectionMultiplexer => _connectionMultiplexer;
}
