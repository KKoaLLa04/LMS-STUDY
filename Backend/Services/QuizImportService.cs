using Backend.Common;
using Backend.DTOs;
using Backend.Services.Interfaces;
using ClosedXML.Excel;

namespace Backend.Services;

// Đọc file Excel theo mẫu cột cố định để tự sinh câu hỏi quiz (kết hợp được với nhập tay —
// xem QuizImportResultDto). Cột: A=Câu hỏi, B-E=Đáp án A-D (C/D/E có thể để trống), F=Đáp án
// đúng (vd "A" hoặc "A,C"), G=Cho phép nhiều đáp án (TRUE/FALSE, để trống thì tự suy ra từ F).
public class QuizImportService : IQuizImportService
{
    private static readonly string[] Headers =
    {
        "Câu hỏi", "Đáp án A", "Đáp án B", "Đáp án C", "Đáp án D", "Đáp án đúng", "Cho phép nhiều đáp án"
    };

    private readonly ILogger<QuizImportService> _logger;

    public QuizImportService(ILogger<QuizImportService> logger)
    {
        _logger = logger;
    }

    public ApiResponse<QuizImportResultDto> ParseExcel(Stream fileStream)
    {
        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var sheet = workbook.Worksheets.First();

            var result = new QuizImportResultDto();
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            var orderNumber = 0;

            for (var row = 2; row <= lastRow; row++)
            {
                var questionText = sheet.Cell(row, 1).GetString().Trim();
                var optionA = sheet.Cell(row, 2).GetString().Trim();
                var optionB = sheet.Cell(row, 3).GetString().Trim();
                var optionC = sheet.Cell(row, 4).GetString().Trim();
                var optionD = sheet.Cell(row, 5).GetString().Trim();
                var correctRaw = sheet.Cell(row, 6).GetString().Trim();
                var allowMultipleRaw = sheet.Cell(row, 7).GetString().Trim();

                // Bỏ qua dòng hoàn toàn trống (không tính là lỗi).
                if (string.IsNullOrWhiteSpace(questionText) && string.IsNullOrWhiteSpace(optionA) &&
                    string.IsNullOrWhiteSpace(optionB) && string.IsNullOrWhiteSpace(correctRaw))
                    continue;

                if (string.IsNullOrWhiteSpace(questionText))
                {
                    result.Warnings.Add($"Dòng {row}: thiếu nội dung câu hỏi, đã bỏ qua");
                    continue;
                }

                var options = new List<(string Letter, string Text)>();
                if (!string.IsNullOrWhiteSpace(optionA)) options.Add(("A", optionA));
                if (!string.IsNullOrWhiteSpace(optionB)) options.Add(("B", optionB));
                if (!string.IsNullOrWhiteSpace(optionC)) options.Add(("C", optionC));
                if (!string.IsNullOrWhiteSpace(optionD)) options.Add(("D", optionD));

                if (options.Count < 2)
                {
                    result.Warnings.Add($"Dòng {row} (\"{questionText}\"): cần ít nhất 2 đáp án, đã bỏ qua");
                    continue;
                }

                var correctLetters = correctRaw
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToUpperInvariant())
                    .ToList();

                if (correctLetters.Count == 0)
                {
                    result.Warnings.Add($"Dòng {row} (\"{questionText}\"): thiếu đáp án đúng, đã bỏ qua");
                    continue;
                }

                var unknownLetters = correctLetters.Where(l => options.All(o => o.Letter != l)).ToList();
                if (unknownLetters.Count > 0)
                {
                    result.Warnings.Add(
                        $"Dòng {row} (\"{questionText}\"): đáp án đúng \"{string.Join(",", unknownLetters)}\" " +
                        "không khớp cột đáp án nào, đã bỏ qua");
                    continue;
                }

                var allowMultiple = allowMultipleRaw switch
                {
                    "TRUE" or "1" => true,
                    "FALSE" or "0" => false,
                    _ => correctLetters.Count > 1
                };

                result.Questions.Add(new QuizQuestionAdminDto
                {
                    Text = questionText,
                    OrderNumber = orderNumber++,
                    AllowMultipleAnswers = allowMultiple,
                    Options = options.Select((o, idx) => new QuizOptionAdminDto
                    {
                        Text = o.Text,
                        IsCorrect = correctLetters.Contains(o.Letter),
                        OrderNumber = idx
                    }).ToList()
                });
            }

            if (result.Questions.Count == 0 && result.Warnings.Count == 0)
                return ApiResponse<QuizImportResultDto>.BadRequest("File không có dữ liệu câu hỏi hợp lệ");

            return ApiResponse<QuizImportResultDto>.Ok(result,
                result.Warnings.Count > 0 ? $"Import thành công, {result.Warnings.Count} dòng bị bỏ qua" : "Import thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đọc file Excel import câu hỏi quiz");
            return ApiResponse<QuizImportResultDto>.BadRequest("File Excel không hợp lệ hoặc sai định dạng mẫu");
        }
    }

    public byte[] BuildTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Câu hỏi");

        for (var i = 0; i < Headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = Headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        sheet.Cell(2, 1).Value = "Thủ đô của Việt Nam là gì?";
        sheet.Cell(2, 2).Value = "Hà Nội";
        sheet.Cell(2, 3).Value = "TP. Hồ Chí Minh";
        sheet.Cell(2, 4).Value = "Đà Nẵng";
        sheet.Cell(2, 6).Value = "A";
        sheet.Cell(2, 7).Value = "FALSE";

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
