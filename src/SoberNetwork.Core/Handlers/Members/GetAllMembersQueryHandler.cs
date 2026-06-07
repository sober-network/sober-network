using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Members;

namespace SoberNetwork.Core.Handlers.Members;

public class GetAllMembersQueryHandler(IMemberService memberService) : IRequestHandler<GetAllMembersQuery, IReadOnlyList<AdminMemberResponse>>
{
    public Task<IReadOnlyList<AdminMemberResponse>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken) =>
        memberService.GetAllMembersAsync(cancellationToken);
}
