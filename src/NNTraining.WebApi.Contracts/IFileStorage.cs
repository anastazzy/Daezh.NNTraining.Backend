﻿using Minio.DataModel;
using NNTraining.Common.Enums;

namespace NNTraining.WebApi.Contracts;

public interface IFileStorage
{
    Task<string> UploadAsync(string fileName, string contentType, Stream fileStream,
        ModelType modelType, Guid idModel, FileType fileType);
    
    Task<ObjectStat> GetAsync(string fileName, ModelType bucketName, string outputFileName = "temp.csv");

    Task<Stream> GetStreamAsync(string fileName, ModelType bucketName);
    
    Task<string?> SaveModel(Guid modelId, string fileName, long size);
}