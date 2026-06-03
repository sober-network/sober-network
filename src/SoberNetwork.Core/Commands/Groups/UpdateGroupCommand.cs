using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record UpdateGroupCommand(string Slug, UpdateGroupRequest Request, string UserId) : IRequest<DataResult<GroupResponse>>;
