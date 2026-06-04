using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetMemberDetailQuery(Guid UserId, string Slug, Guid TargetUserId) : IRequest<DataResult<MemberDetailResponse>>;
