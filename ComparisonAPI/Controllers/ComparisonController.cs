using Microsoft.AspNetCore.Mvc;
using ComparisonAPI.Services;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;

namespace ComparisonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparisonController : ControllerBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IConfiguration _configuration;

        public ComparisonController(IComparisonService comparisonService, IConfiguration configuration)
        {
            _comparisonService = comparisonService;
            _configuration = configuration;
        }

        [HttpGet("selectfolder")]
        public IActionResult SelectFolder()
        {
            try
            {
                // 创建一个新的线程来显示文件夹选择对话框
                string selectedPath = "";
                var thread = new Thread(() =>
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        dialog.Description = "选择保存路径";
                        dialog.UseDescriptionForTitle = true;
                        
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            selectedPath = dialog.SelectedPath;
                        }
                    }
                });

                // 设置线程为单线程单元（STA）
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join(); // 等待线程完成

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    return Ok(new { path = selectedPath });
                }
                
                return BadRequest("未选择路径");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"选择文件夹时出错: {ex.Message}");
            }
        }

        [HttpPost("compare")]
        public async Task<IActionResult> CompareFiles(
            [FromForm] IFormFile fileA, 
            [FromForm] IFormFile fileB, 
            [FromForm] double lengthTolerance, 
            [FromForm] double widthTolerance,
            [FromForm] string savePath)
        {
            try
            {
                var results = await _comparisonService.CompareFiles(fileA, fileB, lengthTolerance, widthTolerance, savePath);
                return Ok(results);
            }
            catch (Exception ex)
            {
                // 检查是否是重复比对的错误
                if (ex.Message.Contains("已经比对过"))
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResults([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            try 
            {
                // 添加日志
                Console.WriteLine($"接收到的时间范围: {startTime} - {endTime}");
                
                // 确保 endTime 包含当天的最后一刻
                endTime = endTime.Date.AddDays(1).AddSeconds(-1);
                
                var results = await _comparisonService.GetResults(startTime, endTime);
                
                // 添加日志
                Console.WriteLine($"查询到 {results.Count} 条记录");
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取结果时出错: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
