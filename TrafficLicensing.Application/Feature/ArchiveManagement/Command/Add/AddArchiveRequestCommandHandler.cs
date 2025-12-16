using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.Add
{
    internal class AddArchiveRequestCommandHandler : ICommandHandler<AddArchiveRequestCommand>
    {
        private readonly IGenericRepository<ArchiveRequests> _repository;

        public AddArchiveRequestCommandHandler(IGenericRepository<ArchiveRequests> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result> Handle(AddArchiveRequestCommand request, CancellationToken cancellationToken)
        {
            var archiveRequest = ArchiveRequests.Create(request.PlateNumber, request.Action, request.Note);
            await _repository.AddAsync(archiveRequest, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}