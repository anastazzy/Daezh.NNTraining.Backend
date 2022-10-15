﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NNTraining.Domain.Dto;

public abstract class ModelInputDto<T> where T: NNParameters
{
    [FromRoute]
    public Guid Id { get; set; }
    
    [FromBody]
    public T? Parameters { get; set; }
}
