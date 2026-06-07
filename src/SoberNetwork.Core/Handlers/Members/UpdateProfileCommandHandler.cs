using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class UpdateProfileCommandHandler(IMemberService memberService) : IRequestHandler<UpdateProfileCommand, DataResult<MemberProfileResponse>>
{
    public async Task<DataResult<MemberProfileResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var (profile, error) = await memberService.UpdateProfileAsync(request.UserId, request.Request, cancellationToken);
        if (error is not null)
            return DataResult<MemberProfileResponse>.Fail(ResultCode.BadRequest, error);
        return DataResult<MemberProfileResponse>.Ok(profile!);
    }
}
