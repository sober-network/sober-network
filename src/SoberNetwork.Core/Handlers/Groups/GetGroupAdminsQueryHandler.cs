using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetGroupAdminsQueryHandler(IMemberService memberService) : IRequestHandler<GetGroupAdminsQuery, DataResult<IReadOnlyList<GroupAdminContactResponse>>>
{
    public async Task<DataResult<IReadOnlyList<GroupAdminContactResponse>>> Handle(GetGroupAdminsQuery request, CancellationToken cancellationToken)
    {
        var (list, error) = await memberService.GetGroupAdminContactsAsync(request.UserId, request.Slug, cancellationToken);
        if (error is not null)
            return DataResult<IReadOnlyList<GroupAdminContactResponse>>.Fail(error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<IReadOnlyList<GroupAdminContactResponse>>.Ok(list!);
    }
}
