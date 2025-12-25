using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Application.Abstraction;
using CRM.Application.Shared.Dto;
using CRM.Domain.Entities;

namespace CRM.Application.Feature.TicketManagement.Command.ChangeTicketStatus
{
    internal sealed class ChangeTicketStatusCommandHandler : ICommandHandler<ChangeTicketStatusCommand>
    {
        private readonly IGenericRepository<Ticket> _repository;
        private readonly IEmailSender _emailSender;

        public ChangeTicketStatusCommandHandler(IGenericRepository<Ticket> repository, IEmailSender emailSender)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (ticket is null)
                return Result.Fail(new Error(
                    Code: "TicketNotFound",
                    Message: " Ticket not found",
                    Type: ErrorType.NotFound
                    ));
            ticket.UpdateStatus(request.Status);
            ticket.UpdateSeverity(request.Severity);
            await _repository.SaveChangesAsync(cancellationToken);

            //4 - Send Email to request.Email
            _emailSender.SendAsync(
          new EmailMessage(
              To: ticket.Email,
              Subject: "Ticket Updated",
              HtmlBody: $"<p>We Updated your ticket  : {request.Description ?? string.Empty} <br></br> New Status: {request.Status} .</p><p><b>Subject:</b> </p>"),
          cancellationToken);

            return Result.Ok();
        }
    }
}