namespace Backend.DTOs;

public class FollowStatusDto
{
    public bool IsFollowing { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
}
