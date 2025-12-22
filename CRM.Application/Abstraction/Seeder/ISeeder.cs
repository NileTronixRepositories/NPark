namespace CRM.Application.Abstraction.Seeder;

public interface ISeeder
{
    public int ExecutionOrder { get; set; }

    Task SeedAsync();
}