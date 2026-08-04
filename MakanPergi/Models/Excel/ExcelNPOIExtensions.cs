using NPOI.SS.UserModel;

namespace MakanPergi.Models.Excel
{
    public static class ExcelNPOIExtensions
    {
        public static string GetCellValue(this IRow row, int cellIndex)
        {
            // GetCell(<cellIndex>) -> returns the CELL (column) at the IRow that we have selected.
            return row.GetCell(cellIndex) == null ? string.Empty : GetCellData(row.GetCell(cellIndex));
        }

        public static string GetCellData(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return cell.NumericCellValue == 0 ? string.Empty: cell.NumericCellValue.ToString();

                default:
                    return cell.StringCellValue;
            }
        }
    }
}
