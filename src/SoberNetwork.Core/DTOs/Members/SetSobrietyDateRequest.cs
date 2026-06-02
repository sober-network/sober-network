using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

public record SetSobrietyDateRequest(
    [Required] DateOnly SobrietyDate
);
