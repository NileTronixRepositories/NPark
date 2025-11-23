using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed class ColletByCachierCommandHandler : ICommandHandler<ColletByCachierCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;

        public ColletByCachierCommandHandler(IGenericRepository<Ticket> ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<Result> Handle(ColletByCachierCommand request, CancellationToken cancellationToken)
        {
            var entity = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken);
            entity!.SetIsCashierCollected();
            await _ticketRepository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}