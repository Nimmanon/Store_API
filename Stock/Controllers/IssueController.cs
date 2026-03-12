using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.Context;
using Stock.Models;
using Stock.Models.ViewModels;
using Stock.Services;
//using Stock.Services;

namespace Stock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly DataContext db;
        private StockService stockService = new StockService();
        public IssueController(DataContext db) => this.db = db;

        [HttpGet("Get")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Issue>>> GetAll()
        {
            try
            {
                var items = await db.Issues.Where(w => w.IsActive).OrderBy(o => o.Product)
                                    .OrderBy(o => o.Product)
                                    .ToListAsync();

                if (items.Count == 0) Ok(new List<Issue>());
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }



        //ทดสอบ qrcode
        //[HttpGet("GetLocationByProduct/{product}")]
        //public async Task<ActionResult> GetLocationsByProduct(string product, int? year = null, int? month = null)
        //{
        //    if (string.IsNullOrWhiteSpace(product))
        //        return BadRequest("Product is required.");

        //    var p = product.Trim();

        //    var q = db.Stocks.Where(s =>
        //        s.Product == p ||
        //        s.Product.StartsWith(p + "-")
        //    );

        //    if (!await q.AnyAsync())
        //        return NotFound($"Product not found: {p}");

        //    int y, m;
        //    if (year.HasValue && month.HasValue)
        //    {
        //        y = year.Value;
        //        m = month.Value;
        //    }
        //    else
        //    {
        //        var latestPeriod = await q.Select(s => s.Year * 100 + s.Month).MaxAsync();
        //        y = latestPeriod / 100;
        //        m = latestPeriod % 100;
        //    }

        //    var result = await q
        //        .Where(s => s.Year == y && s.Month == m)
        //        .GroupBy(s => new { s.Location.Id, s.Location.Name })
        //        .Select(g => new
        //        {
        //            Id = g.Key.Id,
        //            Name = g.Key.Name,
        //            Qty = g.Sum(x => x.BfQty + x.InQty - x.OutQty)
        //        })
        //        .ToListAsync();

        //    return Ok(result);
        //}




        //อันที่ใช้ได้ปกติ
        [HttpGet("GetLocationByProduct/{product}")]
        public async Task<ActionResult> GetLocationsByProduct(
            string product, int? year = null, int? month = null)
        {
            // base query เฉพาะ product นี้
            var q = db.Stocks.Where(s => s.Product == product);

            // เลือก period ที่จะใช้
            int y, m;

            if (year.HasValue && month.HasValue)
            {
                y = year.Value;
                m = month.Value;
            }
            else
            {
                // หา period ล่าสุดจาก Stocks ของ product นี้ เช่น 202512
                var latestPeriod = await q
                    .Select(s => s.Year * 100 + s.Month)
                    .MaxAsync();

                if (latestPeriod == 0)
                    return NotFound();

                y = latestPeriod / 100;
                m = latestPeriod % 100;
            }

            var result = await q
                .Where(s => s.Year == y && s.Month == m)
                .Select(s => new
                {
                    Id = s.Location.Id,
                    Name = s.Location.Name,
                    Qty = s.BfQty + s.InQty - s.OutQty   // ยอดคงเหลือเดือนนี้
                })
                .Distinct()
                .ToListAsync();

            return Ok(result);
        }


        [HttpGet("GetTotalQtyByProduct/{product}")]
        public async Task<ActionResult<int>> GetTotalQtyByProduct(string product)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var total = await db.Stocks
                .AsNoTracking()
                .Where(r => r.Product == product
                            && r.Year == year)
                //&& r.Month == month)
                .SumAsync(r => r.InQty); // หรือ r.Quantity

            return Ok(total);
        }

        //ใช้งานจริง
        [HttpPost("Post")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Issue>> Save(Issue item)
        {
            try
            {
                var location = await db.Locations.FindAsync(item.Location.Id);
                if (location == null) return BadRequest();


                var model = new Issue()
                {
                    EmployeeCode = item.EmployeeCode,
                    Product = item.Product,
                    Location = location,
                    Qty = item.Qty,
                    InputBy = item.InputBy,
                };

                db.Issues.Add(model);
                await db.SaveChangesAsync();

                //keep stock
                List<Models.StockWH> stockList = new List<Models.StockWH>();
                var date = DateTime.Now;
                stockService.addStockOut(date.Year, date.Month, location, item.Product, item.Qty, stockList);
                //update stock
                StockController stock = new StockController(db);
                await stock.StockMove(stockList);
                await db.SaveChangesAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }


    }
}
