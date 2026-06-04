using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record CreateGroupCommand(CreateGroupRequest Request, Guid UserId) : IRequest<DataResult<GroupResponse>>;
