using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Application.Abstraction;
using CRM.Application.Shared.Dto;
using CRM.Domain.Entities;
using CRM.Domain.FileName;
using CRM.Domain.Resources;

namespace CRM.Application.Feature.TicketManagement.Command.Add
{
    internal sealed class AddTicketCommandHandler : ICommandHandler<AddTicketCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<Site> _siteRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IMediaService _mediaService;
        private readonly IEmailSender _emailSender;

        public AddTicketCommandHandler(IGenericRepository<Ticket> ticketRepository, IGenericRepository<Site> siteRepository,
            IGenericRepository<Product> productRepository, IEmailSender emailSender, IMediaService mediaService)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        public async Task<Result> Handle(AddTicketCommand request, CancellationToken cancellationToken)
        {
            //1- cjeck for Site
            var siteExist = await _siteRepository.IsExistAsync(x => x.Id == request.SiteId, cancellationToken);
            if (!siteExist)
                return Result.Fail(new Error(
                    Code: "Ticket.SiteId.NotFound",
                    Message: ErrorMessage.Ticket_Site_NotFound,
                    Type: ErrorType.NotFound
                    ));
            //2-Check for Product
            var productExist = await _productRepository.IsExistAsync(x => x.Id == request.ProductId, cancellationToken);
            if (!productExist)
                return Result.Fail(new Error(
                    Code: "Ticket.ProductId.NotFound",
                    Message: ErrorMessage.Ticket_Product_NotFound,
                    Type: ErrorType.NotFound
                    ));
            //3-Add Ticket
            var ticket = Ticket.Create(request.Description, request.Email, request.Subject, request.PhoneNumber,
                  request.SiteId, request.ProductId);

            //Add Attachments if any
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var file in request.Attachments)
                {
                    var mediaResult = await _mediaService.SaveAsync(file, FileNames.TicketAttachment);

                    var attachment = TicketAttachment.Create(ticket.Id, mediaResult);
                    ticket.AddAttachment(attachment);
                }
            }
            await _ticketRepository.AddAsync(ticket, cancellationToken);
            await _ticketRepository.SaveChangesAsync(cancellationToken);

            //4 - Send Email to request.Email
            await _emailSender.SendAsync(
           new EmailMessage(
               To: request.Email,
               Subject: "Ticket received",
               HtmlBody: $"<p>We received your ticket.</p><p><b>Subject:</b> {request.Subject}</p>"),
           cancellationToken);

            //5-Save Change
            await _ticketRepository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}