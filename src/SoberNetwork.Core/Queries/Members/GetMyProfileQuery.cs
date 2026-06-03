using MediatR;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Queries.Members;

public record GetMyProfileQuery(string UserId) : IRequest<MemberProfileResponse?>;
