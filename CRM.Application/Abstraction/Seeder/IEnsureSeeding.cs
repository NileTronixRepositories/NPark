namespace CRM.Application.Abstraction.Seeder;

public interface IEnsureSeeding
{
    Task SeedDatabaseAsync();
}