using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs;

public record CustomerMessageDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string CustomerEmail,
    int CompanyId,
    string Subject,
    string Message,
    bool IsRead,
    bool IsReplied,
    string? Reply,
    DateTime? RepliedAt,
    int? RepliedByUserId,
    string? RepliedByName,
    MessageCategory Category,
    string CategoryText,
    DateTime CreatedAt
);

public record CustomerMessageCreateDto(
    int CustomerId,
    int CompanyId,
    string Subject,
    string Message,
    MessageCategory Category = MessageCategory.General
);

public record CustomerMessageReplyDto(
    int MessageId,
    string Reply,
    int RepliedByUserId
);

public record CustomerMessageSummaryDto(
    int TotalMessages,
    int UnreadMessages,
    int PendingReplies,
    int RepliedMessages,
    List<CustomerMessageDto> RecentMessages
);
