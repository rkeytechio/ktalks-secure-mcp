namespace Demo.Library.Api.Persistence.Abstractions;

internal interface ICosmosEntity
{
    string PartitionKey { get; }
}
