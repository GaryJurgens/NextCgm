using Medo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextCgm.DataEntities.DocumentDatabases;
using NextCgm.DContentext;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;

namespace NextCgm.Services.Actions
{
    public interface IDocumentDbService
    {
        Task<CreateDocumentDbResponseDTO> CreateDatabaseAsync(CreateDocumentDbRequestDTO request);
    }

    public class DocumentDbService : IDocumentDbService
    {
        private readonly AppDBContext _context;
        private readonly DocumentDbOptions _options;

        public DocumentDbService(AppDBContext context, IOptions<DocumentDbOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<CreateDocumentDbResponseDTO> CreateDatabaseAsync(CreateDocumentDbRequestDTO request)
        {
            try
            {
                if (request.Payload == null || string.IsNullOrEmpty(request.Payload.DatabaseName))
                {
                    return CreateDocumentDbResponseDTO.Failure("Database name is required.");
                }

                var databaseName = request.Payload.DatabaseName;
                
                string baseConnectionString = !string.IsNullOrEmpty(request.Payload.ConnectionString) 
                    ? request.Payload.ConnectionString 
                    : _options.ConnectionString;
                
                // Use MongoUrlBuilder to safely replace or add the database name
                var mongoUrlBuilder = new MongoUrlBuilder(baseConnectionString);
                mongoUrlBuilder.DatabaseName = databaseName;
                string finalConnectionString = mongoUrlBuilder.ToString();

                // Connect to MongoDB
                var client = new MongoClient(finalConnectionString);
                var database = client.GetDatabase(databaseName);
                
                // In MongoDB, a database is not actually created until a collection or document is created.
                // We'll create a dummy collection to ensure the database is physically created.
                var collections = await database.ListCollectionNamesAsync();
                var collectionList = await collections.ToListAsync();
                
                if (!collectionList.Contains("InitCollection"))
                {
                    await database.CreateCollectionAsync("InitCollection");
                }

                // Save to context
                var dbEntity = new UserContainerDatabase
                {
                    UserContainerDatabaseID = Uuid7.NewUuid7(),
                    ClusterName = _options.ClusterName,
                    DatabaseName = databaseName,
                    ConnectionString = finalConnectionString
                };

                await _context.UserContainerDatabases.AddAsync(dbEntity);
                await _context.SaveChangesAsync();

                return new CreateDocumentDbResponseDTO
                {
                    Success = true,
                    Message = "Database created successfully.",
                    Payload = new DocumentDbApiViewModel
                    {
                        DatabaseName = databaseName,
                        ConnectionString = finalConnectionString
                    }
                };
            }
            catch (Exception ex)
            {
                return CreateDocumentDbResponseDTO.Failure($"Failed to create database: {ex.Message}");
            }
        }
    }
}