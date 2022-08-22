﻿using Minio.DataModel;

namespace NNTraining.Contracts;

public interface IFileStorage
{
    Task UploadAsync(string fileName, string contentType, Stream fileStream, long size);
    Task<ObjectStat> GetAsync(string fileName);
}