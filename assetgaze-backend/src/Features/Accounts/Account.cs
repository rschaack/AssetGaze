using LinqToDB.Mapping;

namespace Assetgaze.Backend.Features.Accounts;

[Table("Accounts")]
public class Account
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("Name"), NotNull]
    public string Name { get; set; } = string.Empty;
}