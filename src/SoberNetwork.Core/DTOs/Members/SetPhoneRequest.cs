using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

public record SetPhoneRequest(
    /// <summary>E.164 format recommended, e.g. "+15555550100".</summary>
    [Required, MaxLength(20)] string PhoneNumber
);
