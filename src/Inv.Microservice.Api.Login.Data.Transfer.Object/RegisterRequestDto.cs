namespace Inv.Microservice.Api.Login.Data.Transfer.Object
{
    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string? AdditionalPassword { get; set; } // Solo requerido para Admin
    }
}