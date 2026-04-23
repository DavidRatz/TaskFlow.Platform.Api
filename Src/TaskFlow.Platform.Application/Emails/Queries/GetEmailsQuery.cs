using MediatR;
using TaskFlow.Platform.Domain.Emails.Models;

namespace TaskFlow.Platform.Application.Emails.Queries;

public sealed record GetEmailsQuery(string provider, int MaxEmails = 10) : IRequest<List<EmailMessage>>;
