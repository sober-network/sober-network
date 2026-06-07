using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Members;

namespace SoberNetwork.Core.Handlers.Members;

public class GetUserByIdQueryHandler(IMemberService memberService) : IRequestHandler<GetUserByIdQuery, AdminMemberResponse?>
{
    public Task<AdminMemberResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken) =>
        memberService.GetUserByIdAsync(request.UserId, cancellationToken);
}
