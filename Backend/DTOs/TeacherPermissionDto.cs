using Backend.Models;

namespace Backend.DTOs;

public class TeacherPermissionDto
{
    public PermissionModule Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
