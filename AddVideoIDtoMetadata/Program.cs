using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AddVideoIDtoMetadata
{
    class Program
    {
        private static BlobContainerClient containerClient;

        static async Task Main(string[] args)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            // Get the container and return a container client object
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING_VTUBER");
            BlobServiceClient blobServiceClient = new(connectionString);
            containerClient = blobServiceClient.GetBlobContainerClient("vtuber");

            Console.WriteLine("Listing blobs...");

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"Blob: {blobItem.Name}");

                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                Regex rgx = new Regex(@".*\(([^\(]+)\).*");
                Match match = rgx.Match(blobItem.Name);
                string ID = match.Success? match.Groups[1].Value : "";

                Console.WriteLine($"Id: {ID}");

                Dictionary<string, string> tags = new() { { "id", ID } };

                _ = await blobClient.SetTagsAsync(tags);
                // Cannot modify metadata on archive blobs.
                //_ = await blobClient.SetMetadataAsync(tags);
            }
        }
    }
}
