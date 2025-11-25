namespace BidSphere.Models.Dtos.Products
{
    public class ExcelUploadResultDto
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<FailedRowDto> FailedRows { get; set; } = new List<FailedRowDto>();
    }

    public class FailedRowDto
    {
        public int RowNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
