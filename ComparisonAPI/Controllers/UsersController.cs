using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ComparisonAPI.Models;

namespace ComparisonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IConfiguration configuration, ILogger<UsersController> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"尝试登录用户: {request.Username}");
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "SELECT id, username, password FROM users WHERE username = @Username";
                    var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = request.Username });

                    if (user == null)
                    {
                        _logger.LogWarning($"用户不存在: {request.Username}");
                        return Ok(new LoginResponse 
                        { 
                            Success = false, 
                            Message = "用户名或密码错误" 
                        });
                    }

                    if (user.Password != request.Password)
                    {
                        _logger.LogWarning($"密码错误: {request.Username}");
                        return Ok(new LoginResponse 
                        { 
                            Success = false, 
                            Message = "用户名或密码错误" 
                        });
                    }

                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);

                    _logger.LogInformation($"用户登录成功: {request.Username}");
                    return Ok(new LoginResponse 
                    { 
                        Success = true, 
                        Message = "登录成功" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"登录过程中发生错误: {ex.Message}");
                return StatusCode(500, new LoginResponse 
                { 
                    Success = false, 
                    Message = "服务器错误：" + ex.Message 
                });
            }
        }

        [HttpGet("check")]
        public IActionResult CheckLogin()
        {
            var userId = HttpContext.Session.GetString("UserId");
            return Ok(new { authenticated = !string.IsNullOrEmpty(userId) });
        }
    }
} 