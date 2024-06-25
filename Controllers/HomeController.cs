using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SistemaReservas.Data;
using SistemaReservas.Models;
using SistemaReservas.Recursos;
using System.Diagnostics;
using System.Security.Claims;

namespace SistemaReservas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //Data
        public void GetData()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string Id = "";
            string username = "";
            string roleId = "";
            if (claimUser.Identity.IsAuthenticated)
            {
                Id = claimUser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                username = claimUser.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                roleId = claimUser.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();
            }

            ViewData["Id"] = Id;
            ViewData["username"] = username;
            ViewData["roleId"] = roleId;
        }

        //Index

        public IActionResult Index()
        {
            GetData();

            string role = ViewData["roleId"].ToString();

            if (role == "5")
            {
                return RedirectToAction("Index", "Usuarios");
            }
            else if(role == "6"){
                return RedirectToAction("Home", "Home");
            }

            return View();
        }

        //UserPages

        [Authorize]
        public IActionResult Home()
        {
            GetData();

            return View();
        }

        [Authorize]
        public IActionResult Reserve()
        {
            GetData();

            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Nombre");

            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyReserves()
        {
            GetData();

            string username = ViewData["username"].ToString();

            string Id = ViewData["roleId"].ToString();

            int IdUsu = int.Parse(Id);

            var appDbContext = _context.Reservas.Include(r => r.Usuario).Where(t => t.Usuario.Nombre == username);

            return View(await appDbContext.ToListAsync());
        }

        //Login

        public async Task<Usuario> GetUser(string email, string password)
        {
            Usuario userFound = await _context.Usuario.Where(u => u.Correo == email && u.Clave == password).FirstOrDefaultAsync();

            return userFound;
        }

        public async Task<Usuario> SaveUser(Usuario model)
        {
            _context.Usuario.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario model)
        {
            Usuario userCreated = await SaveUser(model);

            if (userCreated.Id > 0)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewData["Message"] = "User not created";
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario userFound = await GetUser(email, password);

            if (userFound == null)
            {
                ViewData["Message"] = "No matches found";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userFound.Nombre),
                new Claim(ClaimTypes.Role, userFound.RolId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userFound.Id.ToString()),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            if (userFound.RolId == 5)
            {
                return RedirectToAction("Index", "Usuarios");
            }

            return RedirectToAction("Home", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
