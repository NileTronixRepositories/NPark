using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace CRM.Application.Feature.ProductManagement.Command.Add
{
    public class AddProductCommand : ICommand
    {
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public IFormFile? ImagePath { get; init; }
    }
}