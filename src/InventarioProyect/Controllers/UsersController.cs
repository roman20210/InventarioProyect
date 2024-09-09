using Inv.Microservice.Api.Login.Entities;
using Inv.Microservice.Api.Login.Data.Transfer.Object;
using InventarioProyect.Core.Startup.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InventarioProyect.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Suministra acceso al repositorio
        /// </summary>
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequestDto request)
        {
            // Verificar si el nombre de usuario ya existe
            var existingUser = await _context.user
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("El nombre de usuario ya está en uso.");
            }

            // Validar si el tipo de usuario es Admin
            if (request.Role == "Admin")
            {
                var adminPassword = await _context.Adminuser
                    .FirstOrDefaultAsync(a => a.Password == request.AdditionalPassword);

                if (adminPassword == null)
                {
                    return BadRequest("Contraseña adicional para admin incorrecta.");
                }

            }
            else if (request.Role != "Employee")
            {
                return BadRequest("Rol inválido.");
            }

            // Crear nuevo usuario
            var user = new User
            {
                Username = request.Username,
                Password = request.Password,
                Role = request.Role
            };

            _context.user.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado exitosamente." });
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogInUser([FromBody] LogInRequestDto request)
        {
            var user = await _context.user.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }

            // Crear los claims (información que contendrá el token)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("role", user.Role),  // Guardar el rol del usuario en el token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Configurar la clave secreta y las credenciales
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisIsTheSuperSecureKeySaramambices"));  // Asegúrate de que sea segura
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: "IssuerSaramambiches",
                audience: "publicoSaramambiches",
                claims: claims,
                // Duración del token
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            // Devolver el token en la respuesta
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                // Retorna el rol para manejar permisos en el frontend
                role = user.Role,
                username = user.Username
            });
        }

        [Authorize]
        [HttpGet("GetProtectedData")]
        public IActionResult GetProtectedData()
        {
            return Ok("Este es un dato protegido que solo se puede acceder con un token válido.");
        }
    }
}