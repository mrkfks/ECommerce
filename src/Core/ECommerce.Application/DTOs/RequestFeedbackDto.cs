namespace ECommerce.Application.DTOs;

public record RequestFeedbackDto(
    int RequestId,
    string? Feedback,
    int? UserId = null,
    DateTime? FeedbackDate = null
);
