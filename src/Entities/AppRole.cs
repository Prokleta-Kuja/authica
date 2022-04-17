namespace authica.Entities;

public class AppRole
{
    public int AppId { get; set; }
    public int RoleId { get; set; }

    public App? App { get; set; }
    public Role? Role { get; set; }
}