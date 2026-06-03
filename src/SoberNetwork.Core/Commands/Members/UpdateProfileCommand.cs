using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record UpdateProfileCommand(string UserId, UpdateProfileRequest Request) : IRequest<DataResult<MemberProfileResponse>>;
