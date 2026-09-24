using System.ComponentModel.DataAnnotations;
using Innowise.Domain;
namespace Innowise.Api.Assistant;

public sealed class AssistantRequest
{
    public Guid? ConversationId { get; set; }
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = "";

}

public sealed record PaletteChoice(
    Guid PaintId,
    string Role,
    string Reason
);
public sealed record PaletteItem(
    Paint Paint,
    string Role,
    string Reason
);

public sealed record AssistantReply(
    Guid ConversationId,
    string Message,
    IReadOnlyList<PaletteItem> Palette
);
