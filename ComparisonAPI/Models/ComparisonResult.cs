namespace ComparisonAPI.Models
{
    public class ComparisonResult
    {
        // 数据库字段
        public int 序号 { get; set; }
        public string? 编号 { get; set; }  // 添加 ? 表示可为 null
        public double 长度 { get; set; }
        public double 宽度 { get; set; }
        public string? 合格否 { get; set; }  // 添加 ? 表示可为 null
        public string? 超出范围 { get; set; }  // 添加 ? 表示可为 null
        public DateTime 录入时间 { get; set; }
        public string? 文件名 { get; set; }

        // 前端显示字段
        public string? Id { get; set; }  // 添加 ? 表示可为 null
        public double Length { get; set; }
        public double Width { get; set; }
        public string? Status { get; set; }  // 添加 ? 表示可为 null
        public string? Exceeded { get; set; }  // 添加 ? 表示可为 null
        public DateTime EntryTime { get; set; }
        public string? FileName { get; set; }

        // 添加构造函数，设置默认值
        public ComparisonResult()
        {
            编号 = string.Empty;
            合格否 = string.Empty;
            超出范围 = string.Empty;
            Id = string.Empty;
            Status = string.Empty;
            Exceeded = string.Empty;
            录入时间 = DateTime.Now;
            EntryTime = DateTime.Now;
        }
    }
}
