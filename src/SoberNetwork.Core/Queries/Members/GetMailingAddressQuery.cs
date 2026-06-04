using MediatR;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Queries.Members;

public record GetMailingAddressQuery(Guid UserId) : IRequest<MailingAddressResponse?>;
