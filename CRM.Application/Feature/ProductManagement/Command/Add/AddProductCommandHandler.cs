using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Domain.Entities;
using CRM.Domain.FileName;

namespace CRM.Application.Feature.ProductManagement.Command.Add
{
    internal sealed class AddProductCommandHandler : ICommandHandler<AddProductCommand>
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IMediaService _mediaService;

        public AddProductCommandHandler(IGenericRepository<Product> repository, IMediaService mediaService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        public async Task<Result> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            //1- Check NameEn Uniqueness
            var isNameEnExist = await _repository.IsExistAsync(x => x.NameEn == request.NameEn);
            if (isNameEnExist)
                return Result.Fail(new Error(
                    Code: "Product.NameEn.Exist",
                    Message: "Product Name En already exist",
                    Type: ErrorType.Validation
                    ));

            //2-Image Convert
            string? imagePath = null!;
            if (request.ImagePath is not null)
            {
                imagePath = await _mediaService.SaveAsync(request.ImagePath, FileNames.Product);
            }
            //3-Create Product
            var product = Product.Create(request.NameEn, request.NameAr, request.DescriptionEn, request.DescriptionAr, imagePath);
            await _repository.AddAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}