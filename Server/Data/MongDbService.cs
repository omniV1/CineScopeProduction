using MongoDB.Driver;
using Microsoft.Extensions.Options;
using CineScope.Server.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace CineScope.Server.Data
{
    /// <summary>
    /// Service responsible for providing access to MongoDB collections.
    /// Implements the IMongoDbService interface for dependency injection.
    /// </summary>
    public class MongoDbService : IMongoDbService
    {
        /// <summary>
        /// Reference to the MongoDB database instance.
        /// Initialized in the constructor from configuration settings.
        /// </summary>
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Logger for recording operations
        /// </summary>
        private readonly ILogger<MongoDbService> _logger;

        /// <summary>
        /// Initializes a new instance of the MongoDbService.
        /// Sets up the MongoDB client and database connection using injected configuration.
        /// </summary>
        /// <param name="options">MongoDB configuration options from appsettings.json</param>
        /// <param name="logger">Logger for recording operations</param>
        public MongoDbService(IOptions<MongoDbSettings> options, ILogger<MongoDbService> logger, IConfiguration configuration)
        {
            _logger = logger;

            try
            {
                // Extract settings from the injected options
                var settings = options.Value;

                // Get connection string from configuration or options
                // This allows Key Vault to override the connection string with the dash format
                var connectionString = configuration["MongoDbSettings--ConnectionString"] ?? settings.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is empty or not configured.");
                }

                _logger.LogInformation("Initializing MongoDB connection to database: {Database}", settings.DatabaseName);

                // Create MongoDB client settings with logging
                var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

                // Configure MongoDB driver logging (if needed)
                // This is an instance-level configuration
                clientSettings.ClusterConfigurator = builder => {
                    // Add your MongoDB driver logging here if needed
                };

                // Create a new MongoDB client using the settings
                var client = new MongoClient(clientSettings);

                // Get a reference to the specific database for the application
                _database = client.GetDatabase(settings.DatabaseName);

                _logger.LogInformation("Successfully connected to MongoDB database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MongoDB connection");
                throw; // Re-throw to prevent application from starting with invalid DB
            }
        }

        /// <summary>
        /// Gets a MongoDB collection of the specified type by name.
        /// This method provides typed access to database collections.
        /// </summary>
        /// <typeparam name="T">The model type that maps to documents in this collection</typeparam>
        /// <param name="collectionName">The name of the collection in MongoDB</param>
        /// <returns>A typed IMongoCollection instance for performing operations</returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            _logger.LogDebug("Accessing collection: {CollectionName}", collectionName);
            return _database.GetCollection<T>(collectionName);
        }
    }
}