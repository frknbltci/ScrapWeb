﻿namespace ScrapWeb.Utilites.Results
{
    public interface IDataResult<T> : IResult
    {
        T Data { get; }

    }
}
