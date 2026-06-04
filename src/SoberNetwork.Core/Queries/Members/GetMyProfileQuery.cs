using MediatR;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Queries.Members;

public record GetMyProfileQuery(Guid UserId) : IRequest<MemberProfileResponse?>;
