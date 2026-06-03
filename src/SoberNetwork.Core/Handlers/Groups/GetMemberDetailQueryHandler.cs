using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetMemberDetailQueryHandler(IMemberService memberService) : IRequestHandler<GetMemberDetailQuery, DataResult<MemberDetailResponse>>
{
    public async Task<DataResult<MemberDetailResponse>> Handle(GetMemberDetailQuery request, CancellationToken cancellationToken)
    {
        var (member, error) = await memberService.GetMemberInGroupContextAsync(request.UserId, request.Slug, request.TargetUserId);
        if (error is not null)
            return DataResult<MemberDetailResponse>.Fail(error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<MemberDetailResponse>.Ok(member!);
    }
}
