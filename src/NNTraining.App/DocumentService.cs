using Microsoft.AspNetCore.Http;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;

namespace NNTraining.Host;

public class DocumentService : IDocumentService
{
    private readonly NNTrainingDbContext _dbContext;

    public DocumentService(NNTrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<string> UploadDocuments(IEnumerable<IFormFile> files)
    {
        await using var memoryStream = new MemoryStream();
        foreach (var file in files)
        {
            await file.CopyToAsync(memoryStream);
            // Upload the file if less than 2 MB
            if (memoryStream.Length < 2097152)
            {
                var modelFile = new AppFile()
                {
                    Name = file.Name,
                    Content = memoryStream.ToArray()
                };

                _dbContext.Files.Add(modelFile);

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("The file is too large.");
            }
        }
        return "Success";
    }
}