using MediatR;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Queries.Members;

public record GetUserByIdQuery(string UserId) : IRequest<AdminMemberResponse?>;
