using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BlogService.Services
{
    public class MongoDBHealthCheck : IHealthCheck
    {

        private readonly IOptions<MongoDBSettings> _mongoDBSettings;

        public MongoDBHealthCheck(IOptions<MongoDBSettings> mongoDBSettings)
        {
            _mongoDBSettings = mongoDBSettings;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                MongoClient client = new MongoClient(_mongoDBSettings.Value.ConnectionURI);
                IMongoDatabase database = client.GetDatabase(_mongoDBSettings.Value.DatabaseName);
                var collectionName = _mongoDBSettings.Value.CollectionName;

                var collectionNames =await database.ListCollectionNamesAsync(cancellationToken: cancellationToken).Result.ToListAsync(cancellationToken);

                if (collectionNames.Any(cn => cn == collectionName))
                {
                    return HealthCheckResult.Healthy("MongoDB is reachable and has the expected collection.");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("MongoDB is reachable, but the collection does not exist.");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB connection failed.", ex);
            }

        }
    }
}