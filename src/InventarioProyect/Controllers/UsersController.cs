using Inv.Microservice.Api.Login.Entities;
using Inv.Microservice.Api.Login.Data.Transfer.Object;
using InventarioProyect.Core.Startup.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

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
            var user = await _context.user.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }
            var pass = await _context.user.FirstOrDefaultAsync(p => p.Password == request.Password);
            if (pass == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }
            return Ok("Inicio de sesión exitoso.");

        }
    }
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Bienvenido a la API de Inventario");
        }
    }
}