using DocHub.Api.Data;
using DocHub.Api.Data.Models;
using DocHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocHub.Api.Controllers
{
    [Route("Users")]
    public class UserController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole<string>> roleManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserService userService;

        public UserController(AppDbContext context,
                                UserManager<AppUser> userManager,
                                RoleManager<IdentityRole<string>> roleManager,
                                SignInManager<AppUser> signInManager,
                                UserService userService)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(await userManager.FindByEmailAsync(model.Email) is not AppUser user)
            {
                return BadRequest(new
                {
                    message = "Email not found!"
                });
            }

            var result = await userManager.CheckPasswordAsync(user, model.Password);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Wrong Password!",
                    yourModel = model
                });
            }

            await signInManager.SignInAsync(user, true);

            return Ok(new
            {
                message = "user logged in seccussfuly!",
                token = userService.GenerateJsonWebToken(user),
            });
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var Users = context.Users.Select(u => new UserDTO(u)).ToList();
            return Ok(new
            {
                Users,
                count = Users.Count
            }) ;
        }

        [HttpGet("Users/{id}")]
        public async Task<IActionResult> GetUserByIdAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound(new
                {
                    message = "user not found!"
                });
            }
            
            var userRoles = await userManager.GetRolesAsync(user);

            return Ok(new
            {
                info = user,
                userRoles
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            if(await userManager.FindByEmailAsync(model.Email)  != null)
            {
                return BadRequest(new
                {
                    message = "email already used!"
                });
            }

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.UserName,
            };

            var result = await userManager.CreateAsync(user, model.password);

            return Ok(new
            {
                result.Errors,
                message = "user registered seccufuly!"
            });
        }

        //[Authorize]
        [HttpPost("{id}/assign-role")]
        public async Task<IActionResult> AssignRoleAsync(string id , [FromQuery] string roleName)
        {
            var user = await userManager.FindByIdAsync(id);

            if(user == null)
            {
                return NotFound(new
                {
                    message = "user not found!"
                });
            }

            var result = await userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    message = "role added succufuly!"
                });
            }

            return BadRequest(new
            {
                message = "can not assign the role to this user",
                result.Errors
            });
        }

        //[Authorize]
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRoleAsync(string roleName)
        {
            var result = await roleManager.CreateAsync(new IdentityRole<string>()
            {
                Name = roleName,
                Id = Guid.NewGuid().ToString(),
                NormalizedName = roleName.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });

            if(result.Succeeded)
            {
                return Ok(new
                {
                    message = "role created seccufuly!"
                });
            }

            return BadRequest(new
            {
                message = "cano not create role!",
                result.Errors
            });
        }
    }
}
