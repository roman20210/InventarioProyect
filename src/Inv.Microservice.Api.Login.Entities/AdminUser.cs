namespace Inv.Microservice.Api.Login.Entities
{
    public class AdminUser
    {
        public int Id { get; set; }
        public string Password { get; set; } // Contraseña adicional para verificar
    }
}
