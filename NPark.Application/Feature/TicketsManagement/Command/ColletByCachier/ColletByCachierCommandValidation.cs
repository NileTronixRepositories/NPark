using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed class ColletByCachierCommandValidation : AbstractValidator<ColletByCachierCommand>
    {
        public ColletByCachierCommandValidation(IGenericRepository<Ticket> repo)
        {
            RuleFor(x => x.TicketId)
             .NotEmpty()
             .WithMessage(ErrorMessage.TicketId_Required);
        }
    }
}