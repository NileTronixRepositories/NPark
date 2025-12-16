using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.ActionTaken
{
    internal class ActionTakenOnArchiveCommandHandler : ICommandHandler<ActionTakenOnArchiveCommand>
    {
        private readonly IGenericRepository<ArchiveRequests> _repository;

        public ActionTakenOnArchiveCommandHandler(IGenericRepository<ArchiveRequests> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result> Handle(ActionTakenOnArchiveCommand request, CancellationToken cancellationToken)
        {
            var archiveEntity = await _repository.GetByIdAsync(request.Id);
            if (archiveEntity is null)
            {
                return Result.Fail(new Error(
                    Code: "Archive.NotFound",
                    Message: "Archive not found",
                    Type: ErrorType.NotFound
                    ));
            }

            archiveEntity.SetAction(request.ActionTaken, request.RejectReason);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}