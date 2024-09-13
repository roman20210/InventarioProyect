using Inv.Microservice.Api.Login.Entities;
using Inv.Microservice.Api.Login.Data.Transfer.Object;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Inv.Microservice.Api.Login.Entities.Core.Startup.DbContext;
using System.Linq;

namespace InventarioProyect.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Registro de usuario
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequestDto request)
        {
            var existingUser = await _context.user
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("El nombre de usuario ya está en uso.");
            }

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

        // Inicio de sesión
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogInUser([FromBody] LogInRequestDto request)
        {
            var user = await _context.user.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisIsTheSuperSecureKeySaramambices"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "IssuerSaramambiches",
                audience: "publicoSaramambiches",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role,
                username = user.Username
            });
        }

        // Obtener datos protegidos
        [Authorize]
        [HttpGet("GetProtectedData")]
        public IActionResult GetProtectedData()
        {
            return Ok("Este es un dato protegido que solo se puede acceder con un token válido.");
        }

        // Listar usuarios con paginación
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10)
        {
            var totalUsers = await _context.user.CountAsync();
            var users = await _context.user
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                users = users,
                totalItems = totalUsers
            });
        }


        // Obtener un usuario por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.user.FindAsync(id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(user);
        }

        // Actualizar un usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            var user = await _context.user.FindAsync(id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }
            user.Username = request.Username;
            user.Role = request.Role;

            _context.user.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario actualizado exitosamente." });
        }

        // Eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.user.FindAsync(id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            _context.user.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado exitosamente." });
        }
    }
}
