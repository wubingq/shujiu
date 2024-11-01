using ComparisonAPI.Models;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Drawing;
using OfficeOpenXml.Style;

namespace ComparisonAPI.Services
{
    public class ComparisonService : IComparisonService
    {
        private readonly string _connectionString;

        public ComparisonService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<List<ComparisonResult>> CompareFiles(IFormFile fileA, IFormFile fileB, 
            double lengthTolerance, double widthTolerance, string savePath)
        {
            var results = new List<ComparisonResult>();
            
            // 检查文件是否已经比对过
            string fileName = fileB.FileName;
            if (await IsFileAlreadyCompared(fileName))
            {
                throw new Exception($"文件 {fileName} 已经比对过，请勿重复比对");
            }

            using (var streamA = fileA.OpenReadStream())
            using (var streamB = fileB.OpenReadStream())
            using (var packageA = new ExcelPackage(streamA))
            using (var packageB = new ExcelPackage(streamB))
            using (var packageC = new ExcelPackage())
            {
                var sheetA = packageA.Workbook.Worksheets[0]; // 标准文件
                var sheetB = packageB.Workbook.Worksheets[0]; // 待比对文件
                var sheetC = packageC.Workbook.Worksheets.Add("ComparisonResults");

                // 设置表头并调整格式
                sheetC.Cells[1, 1].Value = "编号";
                sheetC.Cells[1, 2].Value = "长度";
                sheetC.Cells[1, 3].Value = "宽度";
                sheetC.Cells[1, 4].Value = "合格否";
                sheetC.Cells[1, 5].Value = "超出范围";
                sheetC.Cells[1, 6].Value = "录入时间";
// 设置表头样式
var headerRange = sheetC.Cells[1, 1, 1, 6];
headerRange.Style.Font.Bold = true;
headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;  
            

                try
                {
                    // 从标准文件（文件A）读取标准值
                    var lengthStr = sheetA.Cells[2, 2].Text?.Trim();
                    var widthStr = sheetA.Cells[2, 3].Text?.Trim();

                    if (string.IsNullOrEmpty(lengthStr) || string.IsNullOrEmpty(widthStr))
                    {
                        throw new Exception("标准文件中的长度或宽度值为空");
                    }

                    if (!double.TryParse(lengthStr, out double standardLength))
                    {
                        throw new Exception($"无法解析标准长度值: {lengthStr}");
                    }

                    if (!double.TryParse(widthStr, out double standardWidth))
                    {
                        throw new Exception($"无法解析标准宽度值: {widthStr}");
                    }

                    Console.WriteLine($"读取到的标准值 - 长度: {standardLength}, 宽度: {standardWidth}");

                    // 遍历待比对文件（文件B）的每一行与标准值进行比对
                    for (int rowB = 2; rowB <= sheetB.Dimension.End.Row; rowB++)
                    {
                        string idB = sheetB.Cells[rowB, 1].Text?.Trim() ?? "";
                        var currentLengthStr = sheetB.Cells[rowB, 2].Text?.Trim();
                        var currentWidthStr = sheetB.Cells[rowB, 3].Text?.Trim();

                        if (!double.TryParse(currentLengthStr, out double lengthB) ||
                            !double.TryParse(currentWidthStr, out double widthB))
                        {
                            continue; // 跳过无效数据
                        }

                        // 计算偏差百分比
                        double lengthDeviation = Math.Abs((lengthB - standardLength) / standardLength * 100);
                        double widthDeviation = Math.Abs((widthB - standardWidth) / standardWidth * 100);

                        // 判断是否在容差范围内
                        bool isQualified = lengthDeviation <= lengthTolerance && 
                                         widthDeviation <= widthTolerance;

                        string status = isQualified ? "合格" : "不合格";
                        string exceeded = isQualified ? "无" : 
                            $"长度超出{lengthDeviation:F2}% 宽度超出{widthDeviation:F2}%";

                        // 创建比对结果
                        var result = new ComparisonResult
                        {
                            编号 = idB,
                            长度 = lengthB,
                            宽度 = widthB,
                            合格否 = status,
                            超出范围 = exceeded,
                            录入时间 = DateTime.Now,
                            文件名 = fileB.FileName,
                            
                            // 前端显示字段
                            Id = idB,
                            Length = lengthB,
                            Width = widthB,
                            Status = status,
                            Exceeded = exceeded,
                            EntryTime = DateTime.Now,
                            FileName = fileB.FileName
                        };

                        // 写入新Excel文件
                        int row = rowB ;  
                        sheetC.Cells[row, 1].Value = idB;
                        sheetC.Cells[row, 2].Value = lengthB;
                        sheetC.Cells[row, 3].Value = widthB;
                        sheetC.Cells[row, 4].Value = status;
                        sheetC.Cells[row, 5].Value = exceeded;
                        sheetC.Cells[row, 6].Value = DateTime.Now;
                        
                      // 设置整行居中对齐
                    var dataRange = sheetC.Cells[row, 1, row, 6];
                    dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        // 设置数值格式
                        sheetC.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                        sheetC.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";

                        // 设置日期格式
                        sheetC.Cells[row, 6].Style.Numberformat.Format = "yyyy/mm/dd hh:mm:ss";

                        // 设置合格/不合格单元格格式
                        if (status == "合格")
                        {
                            sheetC.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheetC.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(198, 239, 206));
                            sheetC.Cells[row, 4].Style.Font.Color.SetColor(Color.FromArgb(0, 97, 0));
                        }
                        else
                        {
                            sheetC.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheetC.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
                            sheetC.Cells[row, 4].Style.Font.Color.SetColor(Color.FromArgb(156, 0, 6));
                        }

                        await SaveToDatabase(result);
                        results.Add(result);
                    }

                    // 在保存文件前自动调整列宽
                    for (int col = 1; col <= 6; col++)
                    {
                        sheetC.Column(col).AutoFit();
                    }
                    // 保存结果文件
                    var resultFileName = $"ComparisonResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var resultFilePath = Path.Combine(savePath, resultFileName);
                    var resultFile = new FileInfo(resultFilePath);
                    if (resultFile.Exists) resultFile.Delete();
                    packageC.SaveAs(resultFile);
                }
                catch (Exception ex)
                {
                    throw new Exception($"比对过程出错: {ex.Message}");
                }
            }

            return results;
        }

        private async Task SaveToDatabase(ComparisonResult result)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    
                    var query = @"INSERT INTO ComparisonResults (编号, 长度, 宽度, 合格否, 超出范围, 录入时间, 文件名) 
                                 VALUES (@编号, @长度, @宽度, @合格否, @超出范围, @录入时间, @文件名);
                                 SELECT SCOPE_IDENTITY();";  // 获新插入行的ID
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        // 添加参数前先清除已有参数
                        command.Parameters.Clear();
                        
                        command.Parameters.AddWithValue("@编号", result.编号);
                        command.Parameters.AddWithValue("@长度", result.长度);
                        command.Parameters.AddWithValue("@宽度", result.宽度);
                        command.Parameters.AddWithValue("@合格否", result.合格否);
                        command.Parameters.AddWithValue("@超出范围", result.超出范围);
                        command.Parameters.AddWithValue("@录入时间", result.录入时间);
                        command.Parameters.AddWithValue("@文件名", result.文件名);
                        
                        // 执行命令并获取返回值
                        var id = await command.ExecuteScalarAsync();
                        if (id != null)
                        {
                            result.序号 = Convert.ToInt32(id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 添加错误日志
                    Console.WriteLine($"保存到数据库时出错: {ex.Message}");
                    throw; // 重新抛出异常以便上层处理
                }
            }
        }

        public async Task<List<ComparisonResult>> GetResults(DateTime startTime, DateTime endTime)
        {
            var results = new List<ComparisonResult>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    var query = @"SELECT 序号, 编号, 长度, 宽度, 合格否, 超出范围, 录入时间, 文件名 
                                 FROM ComparisonResults 
                                 WHERE 录入时间 BETWEEN @StartTime AND @EndTime
                                 ORDER BY 录入时间 DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartTime", startTime);
                        command.Parameters.AddWithValue("@EndTime", endTime);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var result = new ComparisonResult
                                {
                                    序号 = reader.GetInt32(0),
                                    编号 = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    长度 = reader.GetDouble(2),
                                    宽度 = reader.GetDouble(3),
                                    合格否 = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    超出范围 = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    录入时间 = reader.GetDateTime(6),
                                    文件名 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                    
                                    // 添加前端显示字段的映射
                                    Id = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Length = reader.GetDouble(2),
                                    Width = reader.GetDouble(3),
                                    Status = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    Exceeded = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    EntryTime = reader.GetDateTime(6),
                                    FileName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                                };

                                results.Add(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"获取数据时出错: {ex.Message}");
                    throw;
                }
            }

            return results;
        }

        private async Task<bool> IsFileAlreadyCompared(string fileName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(1) FROM ComparisonResults WHERE 文件名 = @fileName";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fileName", fileName);
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。

                    var count = (int)await command.ExecuteScalarAsync();
#pragma warning restore CS8605 // 取消装箱可能为 null 的值。

                    return count > 0;
                }
            }
        }
    }
}
