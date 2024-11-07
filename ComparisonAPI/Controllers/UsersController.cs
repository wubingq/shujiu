using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UsersController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // 这里简单示例，实际应该从数据库验证
        if (model.Username == "admin" && model.Password == "123456")
        {
            // 设置会话或JWT token
            HttpContext.Session.SetString("user", model.Username);
            return Ok(new { success = true, message = "登录成功" });
        }

        return BadRequest(new { success = false, message = "用户名或密码错误" });
    }

    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
        var user = HttpContext.Session.GetString("user");
        return Ok(new { authenticated = !string.IsNullOrEmpty(user) });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { success = true });
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
} 