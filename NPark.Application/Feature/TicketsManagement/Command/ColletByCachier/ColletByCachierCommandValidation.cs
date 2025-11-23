using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed class ColletByCachierCommandValidation : AbstractValidator<ColletByCachierCommand>
    {
        public ColletByCachierCommandValidation(IGenericRepository<Ticket> repo)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, token) => await repo.IsExistAsync(x => x.Id == id, token))
                .WithMessage("Ticket not found");
        }
    }
}