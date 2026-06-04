using MediatR;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Queries.Members;

public record GetUserByIdQuery(Guid UserId) : IRequest<AdminMemberResponse?>;
