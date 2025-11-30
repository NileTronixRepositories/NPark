using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace NPark.Application.Feature.ScannerManagement.Command
{
    public sealed record GetScannerInfoCommand : ICommand
    {
        public IFormFile Photo { get; set; } = null!;
    }
}