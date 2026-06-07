using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Members;

namespace SoberNetwork.Core.Handlers.Members;

public class GetMailingAddressQueryHandler(IMemberService memberService)
    : IRequestHandler<GetMailingAddressQuery, MailingAddressResponse?>
{
    public Task<MailingAddressResponse?> Handle(GetMailingAddressQuery request, CancellationToken cancellationToken) =>
        memberService.GetMailingAddressAsync(request.UserId, cancellationToken);
}
