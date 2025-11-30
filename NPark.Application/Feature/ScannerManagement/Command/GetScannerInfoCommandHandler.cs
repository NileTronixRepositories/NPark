using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using NPark.Domain.FileNames;

namespace NPark.Application.Feature.ScannerManagement.Command
{
    public sealed class GetScannerInfoCommandHandler : ICommandHandler<GetScannerInfoCommand>
    {
        private readonly IMediaService _mediaService;

        public GetScannerInfoCommandHandler(IMediaService mediaService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        public async Task<Result> Handle(GetScannerInfoCommand request, CancellationToken cancellationToken)
        {
            string filePath = string.Empty;
            if (request.Photo is not null)
            {
                filePath = await _mediaService.SaveAsync(request.Photo, FileNames.ScannerPhotos);
            }

            return Result.Ok();
        }
    }
}