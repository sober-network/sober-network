using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetMemberDetailQuery(string UserId, string Slug, string TargetUserId) : IRequest<DataResult<MemberDetailResponse>>;
