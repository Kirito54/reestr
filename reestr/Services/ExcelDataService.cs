using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace reestr.Services;

public record ReestrEntry(string QueueNumber, string MfcNumber, string OrderWithDate);

public class ExcelDataService
{
    private readonly List<ReestrEntry> _entries = new();
    public IReadOnlyList<ReestrEntry> Entries => _entries;

    public void LoadFromFile(string path)
    {
        using var wb = new XLWorkbook(path);
        LoadFromWorkbook(wb);
    }

    public void LoadFromStream(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        LoadFromWorkbook(wb);
    }

    private void LoadFromWorkbook(XLWorkbook wb)
    {
        var ws = wb.Worksheets.First();
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var queue = row.Cell(1).GetString();
            var mfc = row.Cell(2).GetString();
            var order = row.Cell(3).GetString();
            if (ContainsPersonalData(queue) || ContainsPersonalData(mfc) || ContainsPersonalData(order))
                throw new InvalidOperationException("Файл содержит персональные данные");
            _entries.Add(new ReestrEntry(queue, mfc, order));
        }
    }

    private static bool ContainsPersonalData(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        // Simple heuristics: FIO pattern or long numbers
        var fioPattern = new Regex(@"^[А-ЯЁ][а-яё]+\s+[А-ЯЁ][а-яё]+(\s+[А-ЯЁ][а-яё]+)?");
        var passportPattern = new Regex(@"\b\d{10}\b");
        var snilsPattern = new Regex(@"\b\d{11}\b");
        return fioPattern.IsMatch(value) || passportPattern.IsMatch(value) || snilsPattern.IsMatch(value);
    }

    public IEnumerable<ReestrEntry> FindByOrder(string orderNumber)
        => _entries.Where(e => e.OrderWithDate.Contains(orderNumber, StringComparison.OrdinalIgnoreCase));

    public ReestrEntry? FindByOrderAndMfc(string orderNumber, string mfc)
        => _entries.FirstOrDefault(e => e.OrderWithDate.Contains(orderNumber, StringComparison.OrdinalIgnoreCase) && e.MfcNumber == mfc);
}
