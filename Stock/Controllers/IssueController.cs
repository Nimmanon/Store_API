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
