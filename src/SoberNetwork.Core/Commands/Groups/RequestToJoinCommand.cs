using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record RequestToJoinCommand(string Slug, Guid UserId) : IRequest<DataResult<bool>>;
