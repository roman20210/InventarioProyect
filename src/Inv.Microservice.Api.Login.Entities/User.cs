namespace Inv.Microservice.Api.Login.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; } // Puede ser "Admin" o "Employee"
    }
}