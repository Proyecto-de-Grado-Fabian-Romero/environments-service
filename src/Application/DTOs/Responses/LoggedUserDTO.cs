namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class LoggedUserDTO
{
    public Guid PublicId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public bool VerifiedEmail { get; set; }

    public bool VerifiedPhone { get; set; }

    required public string Role { get; set; }

    public string? PhotoFileId { get; set; }

    public string? PhotoFileName { get; set; }

    public string? PhotoFileUrl { get; set; }

    public bool? Verified { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
