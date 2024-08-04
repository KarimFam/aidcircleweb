namespace H4H.Application.DTOs
{
    public class UserDto
    {
    //    public Guid? UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public string ExternalAuthProvider { get; set; }
        public string ExternalAuthId { get; set; }

        public Guid? OrganizationId { get; set; }

    }
}
