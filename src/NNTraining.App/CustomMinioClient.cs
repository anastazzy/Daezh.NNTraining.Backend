using Minio;
using Minio.Exceptions;

namespace NNTraining.Host;

class CustomMinioClient
{
    static void Main(string[] args)
    {
        var endpoint  = "";
        var accessKey = "Q3AM3UQ867SPQQA43P2F";
        var secretKey = "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG";
        
        try
        {
            var minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey,
                    secretKey)
                .WithSSL()
                .Build();
            Run(minio).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.ReadLine();
    }

    // File uploader task.
    private async static Task Run(MinioClient minio)
    {
        var bucketName = "mymusic";
        var location   = "us-east-1";
        var objectName = "golden-oldies.zip";
        var filePath = "C:\\Users\\username\\Downloads\\golden_oldies.mp3";
        var contentType = "application/zip";

        try
        {
            // Make a bucket on the server, if not already present.
            bool found = await minio.BucketExistsAsync(bucketName);
            if (!found)
            {
                await minio.MakeBucketAsync(bucketName, location);
            }
            // Upload a file to bucket.
            await minio.PutObjectAsync(bucketName, objectName, filePath, contentType);
            Console.WriteLine("Successfully uploaded " + objectName );
        }
        catch (MinioException e)
        {
            Console.WriteLine("File Upload Error: {0}", e.Message);
        }
    }
}