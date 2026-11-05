namespace AEPW.Application.DTOs.User
{
    public class DeleteUserResponseDto
    {
        public long UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasPendingReviews { get; set; }
        public string? PendingReviewIds { get; set; }
    }
}
