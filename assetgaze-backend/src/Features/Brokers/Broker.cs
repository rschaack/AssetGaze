using LinqToDB.Mapping;

namespace Assetgaze.Backend.Features.Brokers;

[Table("Brokers")]
public class Broker
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("Name"), NotNull]
    public string Name { get; set; } = string.Empty;
}