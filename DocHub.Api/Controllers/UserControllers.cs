using DocHub.Api.Data;
using DocHub.Api.Data.Models;
using DocHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocHub.Api.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<AppUser> userManager;
        //private readonly RoleManager<AppUser> roleManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserService userService;

        public UserController(AppDbContext context,
                                UserManager<AppUser> userManager,
                                //RoleManager<AppUser> roleManager,
                                SignInManager<AppUser> signInManager,
                                UserService userService)
        {
            this.context = context;
            this.userManager = userManager;
            //this.roleManager = roleManager;
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

            //var result = await signInManager.CheckPasswordSignInAsync(user, model.Password,false);
            var result = await userManager.CheckPasswordAsync(user, model.Password);
            //Console.WriteLine(result.ToString());
            //if (!result)
            //{
            //    return BadRequest(new
            //    {
            //        message = "Wrong Password!",
            //        yourModel = model
            //    });
            //}

            //await signInManager.SignInAsync(user,true);

            //return Ok(new
            //{
            //    message = "user logged in seccussfuly!",
            //    token = userService.GenerateJsonWebToken(user),
            //});
            return Ok(new { result });
        }

        [HttpGet("Users")]
        public IActionResult GetAllUsers()
        {
            return Ok(context.Users.ToArray());
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

            await userManager.CreateAsync(user,model.password);
            //await userManager.AddPasswordAsync(user, model.password);

            return Ok(new
            {
                message = "user registered seccufuly!"
            });
        }

        [Authorize]
        [HttpPost("{id}/assign-role")]
        public IActionResult AssignRole(string id)
        {
            throw new NotImplementedException();
        }
    }
}
