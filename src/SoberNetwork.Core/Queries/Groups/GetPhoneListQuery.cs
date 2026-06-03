using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetPhoneListQuery(string UserId, string Slug) : IRequest<DataResult<IReadOnlyList<PhoneListEntryResponse>>>;
