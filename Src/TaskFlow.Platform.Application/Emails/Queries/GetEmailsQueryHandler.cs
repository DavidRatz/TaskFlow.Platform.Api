using MediatR;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;

namespace TaskFlow.Platform.Application.Emails.Queries;

public sealed class GetEmailsQueryHandler(IEmailConnector emailConnector)
    : IRequestHandler<GetEmailsQuery, List<EmailMessage>>
{
    /**
     * Tache de gestion pour la récupération des emails
     */
    public async Task<List<EmailMessage>> Handle(GetEmailsQuery request, CancellationToken cancellationToken)
    {
        return await emailConnector.GetEmailsAsync(request.provider, request.MaxEmails, cancellationToken);
    }
}
