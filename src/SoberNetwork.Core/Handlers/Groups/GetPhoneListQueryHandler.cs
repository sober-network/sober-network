using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetPhoneListQueryHandler(IMemberService memberService) : IRequestHandler<GetPhoneListQuery, DataResult<IReadOnlyList<PhoneListEntryResponse>>>
{
    public async Task<DataResult<IReadOnlyList<PhoneListEntryResponse>>> Handle(GetPhoneListQuery request, CancellationToken cancellationToken)
    {
        var (list, error) = await memberService.GetGroupPhoneListAsync(request.UserId, request.Slug);
        if (error is not null)
            return DataResult<IReadOnlyList<PhoneListEntryResponse>>.Fail(error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<IReadOnlyList<PhoneListEntryResponse>>.Ok(list!);
    }
}
