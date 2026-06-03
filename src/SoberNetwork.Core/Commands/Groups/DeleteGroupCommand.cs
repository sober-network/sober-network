using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record DeleteGroupCommand(string Slug, string UserId) : IRequest<CommandResult>;
