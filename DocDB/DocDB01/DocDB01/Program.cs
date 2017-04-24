using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace DocDB01
{
    public class Family
    {
        public string id { get; set; }
        public string lastName { get; set; }
        public string surName { get; set; }
        public List<Child> children { get; set; }
    }
    public class Child
    {
        public string firstName { get; set; }
        public List<Pet> pets { get; set; }
    }
    public class Pet
    {
        public string givenName { get; set; }
    }

    class Program
    {
        public static string docDBEndpoint = "https://localhost:8081";
        public static string docDBName = "localdocdb";
        public static string colName = "personcol";
        public static string userName = "mobileuser";
        public static string permissionName = "readperm";
        public static string partitionKey = null;
        public static int throughPut = 400;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Execute(args);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }).Wait();
        }

        public static async Task Execute(string[] args)
        {
            try
            {
                DocumentClient client = await ConnectAsync(new Uri(docDBEndpoint), GetMasterKey());
                Database docDB = await CreateOrGetDatabase(client, docDBName);
                DocumentCollection col = await CreateOrGetCollection(
                    client,
                    docDB,
                    colName,
                    throughPut,
                    partitionKey,
                    Program.DefineCustomIndex, false);

                // Show Ressource Token Sample
                await ShowRessourceTokenDemo(client, col);
                // Create a simple object invoking a trigger
                #region Create Document with trigger
                try
                {
                    RequestOptions options = new RequestOptions();
                    options.PreTriggerInclude = new List<string>();
                    options.PreTriggerInclude.Add("validateDocumentContents");
                    await client.CreateDocumentAsync(col.SelfLink,
                        new { id = "HubsiMayer" }, options
                        );
                }
                catch (Exception createException)
                {
                    Console.WriteLine($"Failed to create document:{createException.Message}");
                }
                #endregion
                // Untyped Linq-Queries
                #region Untyped Linq-Queries
                var query = client.CreateDocumentQuery(col.SelfLink, new FeedOptions()
                {
                    MaxItemCount = 2
                });

                var docQuery = query.AsDocumentQuery(); // Supports Pagination and Async

                while (docQuery.HasMoreResults)
                {
                    foreach (dynamic b in await docQuery.ExecuteNextAsync())
                    {
                        Console.WriteLine("item");
                    }
                }

                var itemQuery = (query.Where(d => d.Id == "Andreas")).AsDocumentQuery();
                string sqlStatement = itemQuery.ToString();

                var item = (await itemQuery.ExecuteNextAsync()).FirstOrDefault();
                #endregion
                #region Select Many Query <Family>

                var familiesChildrenAndPets = client.CreateDocumentQuery<Family>(col.SelfLink)
                    .SelectMany(family => family.children
                        .SelectMany(child => child.pets
                            .Where(pet => pet.givenName == "Fluffy")
                                .Select(pet => new
                                {
                                    family = family.id,
                                    child = child.firstName,
                                    pet = pet.givenName
                                }
                                )
                         )
                    );
                sqlStatement = familiesChildrenAndPets.ToString();

                #endregion
                // Typed Linq Queries
                #region Typed Linq-Queries
                var queryTyped = client.CreateDocumentQuery<Family>(col.SelfLink);
                var docQueryTyped = queryTyped.AsDocumentQuery<Family>();

                while (docQueryTyped.HasMoreResults)
                {
                    foreach (Family b in await docQueryTyped.ExecuteNextAsync())
                    {
                        Console.WriteLine("Ttem");
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message} ");
            }
        }

        public static async Task ShowRessourceTokenDemo(DocumentClient client, DocumentCollection col)
        {
            string collectionSelfLink = col.SelfLink;

            User docUser = null;
            try
            {
                #region Create a user
                try
                {
                    docUser = await client.ReadUserAsync(UriFactory.CreateUserUri(docDBName, userName));
                }
                catch (Exception) { }
                if (docUser == null)
                {
                    docUser = new User
                    {
                        Id = userName
                    };
                    docUser = await client.CreateUserAsync(UriFactory.CreateDatabaseUri(docDBName), docUser);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create user! Message:{ex.Message} ");
            }

            Permission docPermission = null;
            try
            {
                #region Create or read permission (read permission also creates a new token).
                FeedResponse<Permission> perms = await client.ReadPermissionFeedAsync(
                  UriFactory.CreateUserUri(docDBName, docUser.Id));
                var readPermission = perms.Where(p => p.Id == permissionName).FirstOrDefault();
                docPermission = new Permission
                {
                    PermissionMode = PermissionMode.Read,
                    ResourceLink = col.SelfLink,
                    Id = permissionName
                };
                if (readPermission == null)
                {
                    docPermission = await client.CreatePermissionAsync(UriFactory.CreateUserUri(docDBName, userName), docPermission);
                    Console.WriteLine("Created permission");
                }
                else
                {
                    docPermission = readPermission;
                    Console.WriteLine("Read permission (updated Token)");
                }
                #endregion
                Console.WriteLine(docPermission.Id + " has token of: " + docPermission.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create permission for user! Message:{ex.Message} ");
            }

            // Simulate Permission Transmission
            List<Permission> permList = new List<Permission>();
            MemoryStream memStream = new MemoryStream();
            docPermission.SaveTo(memStream);
            memStream.Position = 0L;
            permList.Add(Permission.LoadFrom<Permission>(memStream));

            // Create a UserClient with Resource-Token
            DocumentClient userClient = new DocumentClient(new Uri(docDBEndpoint), permList);
            var resource = await userClient.ReadDocumentCollectionAsync(collectionSelfLink);
            var userCollection = resource.Resource;

            // Read data
            var userQuery = client.CreateDocumentQuery(col.SelfLink, new FeedOptions() { MaxItemCount = 10 });
            var userResult = userQuery.AsEnumerable().ToList();

            // Create data fails as we do not have write access
            try
            {
                await userClient.CreateDocumentAsync(userCollection.SelfLink,
                new { id = "UserCreatedObject" }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create document for user! Message:{ex.Message} ");
            }
        }

        public static async Task<DocumentClient> ConnectAsync(Uri docDBEndpoint, SecureString key)
        {
            DocumentClient client = new DocumentClient(docDBEndpoint, key);
            await client.OpenAsync();
            return client;
        }

        public static SecureString GetMasterKey()
        {
            // Do not do this in production - Store the key in AzureKeyVault & Create RessourceKeys
            SecureString key = new SecureString();
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==".ToCharArray().ToList().ForEach(p => key.AppendChar(p));
            return key;
        }

        public static async Task<Database> CreateOrGetDatabase(DocumentClient client, string docDBName)
        {
            Database docDB = client.CreateDatabaseQuery()
                        .Where(d => d.Id == docDBName)
                        .AsEnumerable()
                        .FirstOrDefault();

            if (docDB == null)
                docDB = await client.CreateDatabaseAsync(new Database { Id = docDBName });
            return docDB;
        }

        public static async Task<DocumentCollection> CreateOrGetCollection(
            DocumentClient client,
            Database docDB,
            string colName,
            int throughPut,
            string partitionKey,
            Func<DocumentCollection, IndexingPolicy> defineCustomIndex,
            bool indexChanged)
        {
            DocumentCollection docDBCollection = new DocumentCollection();

            docDBCollection = client.CreateDocumentCollectionQuery(docDB.SelfLink)
                            .Where(c => c.Id == colName)
                            .AsEnumerable()
                            .FirstOrDefault();

            if (docDBCollection == null)
            {
                docDBCollection = new DocumentCollection() { Id = colName };
                var requestOptions = new RequestOptions
                {
                    OfferThroughput = throughPut
                };
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    docDBCollection.PartitionKey.Paths.Add(partitionKey);
                }
                defineCustomIndex?.Invoke(docDBCollection);
                docDBCollection = await client.CreateDocumentCollectionAsync(docDB.SelfLink, docDBCollection, requestOptions);
            }
            else
            {
                if (indexChanged)
                {
                    docDBCollection.IndexingPolicy = (defineCustomIndex?.Invoke(docDBCollection)) ?? docDBCollection.IndexingPolicy;
                    docDBCollection = await client.ReplaceDocumentCollectionAsync(docDBCollection);
                }
            }
            return docDBCollection;
        }

        private static IndexingPolicy DefineCustomIndex(DocumentCollection col)
        {
            return null;
        }
    }
}