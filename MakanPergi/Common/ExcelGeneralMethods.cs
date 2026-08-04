using Common.Model.Excel;
using MakanPergi.Models.Excel;
using NPOI.SS.UserModel;

namespace MakanPergi.Common
{
    public static class ExcelGeneralMethods
    {
        public static void AddError(List<UploadExcelResult> uploadExcelResult, IRow currentRow, 
                                    int cellIndex, int rowIndex, string errorMessage)
        {
            uploadExcelResult.Add(new UploadExcelResult
            {
                EntityName = currentRow.GetCellValue(cellIndex),
                Message = $"Error at row {rowIndex}: {errorMessage}.",
                IsValid = false
            });
        }

        public static void AddSuccess(List<UploadExcelResult> uploadExcelResult, int cellIndex, IRow currentRow)
        {
            uploadExcelResult.Add(new UploadExcelResult
            {
                EntityName = currentRow.GetCellValue(cellIndex),
                IsValid = true
            });
        }

        public static string GetCellStringValue(IRow currentRow, int cellIndex)
        {
            return currentRow.GetCellValue(cellIndex);
        }
    }
}
