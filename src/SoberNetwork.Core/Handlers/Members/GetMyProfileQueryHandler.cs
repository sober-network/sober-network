using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Members;

namespace SoberNetwork.Core.Handlers.Members;

public class GetMyProfileQueryHandler(IMemberService memberService) : IRequestHandler<GetMyProfileQuery, MemberProfileResponse?>
{
    public Task<MemberProfileResponse?> Handle(GetMyProfileQuery request, CancellationToken cancellationToken) =>
        memberService.GetMyProfileAsync(request.UserId, cancellationToken);
}
