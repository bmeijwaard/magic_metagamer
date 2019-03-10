using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using The_MTG_Metagamer_Shared.Models;

namespace The_MTG_Metagamer_Shared.Builders
{
    public static class ExcelBuilder
    {
        public static void Build(IEnumerable<Deck> decks, List<Models.Single> singles, Format format)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                ExcelWorksheet worksheet;
                if (excel.Workbook.Worksheets.Count > 0)
                    excel.Workbook.Worksheets.Delete(1);

                var decksListSheet = excel.Workbook.Worksheets.Add($"Decks list");
                var singlesListSheet = excel.Workbook.Worksheets.Add($"Singles list");

                var deck_headerCells = decksListSheet.Cells[1, 1, 1, 3];
                var deck_headerFont = deck_headerCells.Style.Font;
                deck_headerFont.SetFromFont(new Font("Calibri", 14, FontStyle.Bold));
                var deck_headerFill = deck_headerCells.Style.Fill;
                deck_headerFill.PatternType = ExcelFillStyle.Solid;
                deck_headerFill.BackgroundColor.SetColor(Color.LightGray);

                var deck_border = deck_headerCells.Style.Border;
                deck_border.Bottom.Style = ExcelBorderStyle.Thin;

                decksListSheet.Cells[1, 1].Value = "Decks scraped";
                decksListSheet.Cells[1, 2].Value = "Cardkingdom price";
                decksListSheet.Cells[1, 3].Value = "Magiccardmarket price";

                var count = 2;
                foreach (var deck in decks.OrderBy(d => d.Name))
                {
                    Console.WriteLine($"Write to excel: {deck.Name}");
                    var sheetName = $"{count}_{deck.Name}";
                    decksListSheet.Cells[count, 1].Value = deck.Name;
                    Uri url = new Uri($"#'{sheetName}'!A1", UriKind.Relative);
                    decksListSheet.Cells[count, 1].Hyperlink = url;
                    decksListSheet.Cells[count, 1].Style.Font.Color.SetColor(Color.DarkBlue);
                    decksListSheet.Cells[count, 1].Style.Font.UnderLine = true;

                    decksListSheet.Cells[count, 2].Value = deck.Cards.Select(c => c.CKD_Price * c.Copies).Sum();
                    decksListSheet.Cells[count, 3].Value = deck.Cards.Select(c => c.MCM_Price * c.Copies).Sum();

                    worksheet = excel.Workbook.Worksheets.Add(sheetName);
                    count++;

                    var headerCells = worksheet.Cells[1, 1, 1, 7];
                    var headerFont = headerCells.Style.Font;
                    headerFont.SetFromFont(new Font("Calibri", 14, FontStyle.Bold));
                    var headerFill = headerCells.Style.Fill;
                    headerFill.PatternType = ExcelFillStyle.Solid;
                    headerFill.BackgroundColor.SetColor(Color.LightGray);

                    var border = headerCells.Style.Border;
                    border.Bottom.Style = ExcelBorderStyle.Thin;

                    worksheet.Cells[1, 1].Value = "Card";
                    worksheet.Cells[1, 2].Value = "Copies";
                    worksheet.Cells[1, 3].Value = "CKD Price PP";
                    worksheet.Cells[1, 4].Value = "CKD Price Total";
                    worksheet.Cells[1, 5].Value = "MCM Price PP";
                    worksheet.Cells[1, 6].Value = "MCM Price Total";
                    worksheet.Cells[1, 7].Value = "Card Type";


                    var row = 2;
                    foreach (var card in deck.Cards.OrderBy(c => c.CardType).ThenBy(c => c.Name))
                    {
                        var mcmPrice = singles.First(s => s.Name == card.Name).MCM_Price;
                        worksheet.Cells[row, 1].Value = card.Name;
                        worksheet.Cells[row, 2].Value = card.Copies;
                        worksheet.Cells[row, 3].Value = card.CKD_Price;
                        worksheet.Cells[row, 4].Value = Convert.ToDouble(card.Copies * card.CKD_Price);
                        worksheet.Cells[row, 5].Value = mcmPrice;
                        worksheet.Cells[row, 6].Value = Convert.ToDouble(card.Copies * mcmPrice);
                        worksheet.Cells[row, 7].Value = card.CardType.ToString();

                        row++;
                    }

                    worksheet.Cells[row, 4].Formula = $"=SUM(D2:D{row - 1})";
                    worksheet.Cells[row, 6].Formula = $"=SUM(F2:F{row - 1})";
                    worksheet.Cells[row, 7].Value = "Sum";

                    worksheet.Cells.AutoFitColumns();
                }

                ExcelAddress range = new ExcelAddress(2, 2, count, 2);
                var rule = decksListSheet.ConditionalFormatting.AddThreeColorScale(range);
                rule.Priority = 1;
                rule.LowValue.Color = Color.FromArgb(0xFF, 0x63, 0xBE, 0x7B);
                rule.HighValue.Color = Color.FromArgb(0xFF, 0xF8, 0x69, 0x6B);
                rule.MiddleValue.Color = Color.FromArgb(0xFF, 0xFF, 0xEB, 0x84);

                decksListSheet.Cells.AutoFitColumns();


                var single_headerCells = singlesListSheet.Cells[1, 1, 1, 3];
                var single_headerFont = single_headerCells.Style.Font;
                single_headerFont.SetFromFont(new Font("Calibri", 14, FontStyle.Bold));
                var single_headerFill = single_headerCells.Style.Fill;
                single_headerFill.PatternType = ExcelFillStyle.Solid;
                single_headerFill.BackgroundColor.SetColor(Color.LightGray);

                var single_border = single_headerCells.Style.Border;
                single_border.Bottom.Style = ExcelBorderStyle.Thin;

                singlesListSheet.Cells[1, 1].Value = "Single";
                singlesListSheet.Cells[1, 2].Value = "Cardkingdom price";
                singlesListSheet.Cells[1, 3].Value = "Magiccardmarket price";

                var r = 2;
                foreach(var single in singles)
                {
                    singlesListSheet.Cells[r, 1].Value = single.Name;
                    singlesListSheet.Cells[r, 2].Value = single.CKD_Price;
                    singlesListSheet.Cells[r, 3].Value = single.MCM_Price;

                    r++;
                }
                singlesListSheet.Cells.AutoFitColumns();

                excel.Workbook.Properties.Title = "MTG GoldFish Legacy Extract";
                excel.Workbook.Properties.Author = "Bob Meijwaard";
                excel.Workbook.Properties.Company = "Bob Meijwaard";

                FileInfo excelFile = new FileInfo($@"C:\temp\mtggoldfish_{format}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                excel.SaveAs(excelFile);
            }
        }
    }
}
