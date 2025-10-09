using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.Context;
using Stock.Models;
using Stock.Models.ViewModels;
using Stock.Services;

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
                var items = await db.Issues.Where(w => w.IsActive).OrderBy(o => o.Product).ToListAsync();

                if (items.Count == 0) return Ok(new List<Issue>());
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }



        [HttpGet("GetLocationByProduct/{product}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<string>>> GetLocationsByWorkOrder(string product)
        {
            try
            {
                var item = await db.Stocks
                    .Where(r => r.Product == product &&
            r.Year == DateTime.Now.Year &&
            r.Month == DateTime.Now.Month)


                     .Select(r => new
                     {
                         Id = r.Location.Id,
                         Name = r.Location.Name
                     })
                    .Distinct()
                    .ToListAsync();

                return Ok(item);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }


        [HttpGet("GetById/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Issue>>> GetDataById(int id)
        {
            try
            {
                var item = await db.Issues.Where(w => w.Id == id).SingleOrDefaultAsync();

                if (item == null) return Ok(new Issue());
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }      


        [HttpPost("Post")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Issue>> Save(IssueVM item)
        {
            try

            {
                var location = await db.Locations.FindAsync(item.Location.Id);
                if (location == null) return BadRequest();

                //var area = await db.Areas.FindAsync(item.Area.Id);
                //if (area == null) return BadRequest();

                //var group = await db.Groups.FindAsync(item.Group.Id);
                //if (group == null) return BadRequest();

                //var status = await db.Statuses.FindAsync(item.Status.Id);
                //if (status == null) return BadRequest();

                //var unit = await db.Units.FindAsync(item.Unit.Id);
                //if (unit == null) return BadRequest();

                var Issue = new Issue()

                {
                    EmployeeCode = item.EmployeeCode,
                    //WorkOrder = item.WorkOrder,
                    Location = location,
                    //Area = area,
                    //Group = group,
                    //Status = status,
                    //Unit = unit,
                    Qty = item.Qty,
                    IsActive = true,
                    InputBy = item.InputBy,
                };
                db.Issues.Add(Issue);


                //update wo controll
                if (item.Qty > 0)
                {
                    var wc = await db.ProductControls.Where(x => x.IsActive && x.Pd == item.Product).FirstOrDefaultAsync();
                    if (wc != null)
                    {
                        wc.IssueQty += item.Qty;

                        wc.IsIssued = wc.Qty == wc.IssueQty ? true : false;
                    }

                    await db.SaveChangesAsync();
                }

                //keep stock
                List<Models.StockWH> stockList = new List<Models.StockWH>();
                var date = DateTime.Now;
                stockService.addStockOut(date.Year, date.Month, location,  item.Product, item.Qty, stockList);

                //update stock
                StockController stock = new StockController(db);
                await stock.StockMove(stockList);
                await db.SaveChangesAsync();

                return Ok(Issue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Issue>> Remove(ActionVM item)
        {
            try
            {
                var Issue = await db.Issues.FindAsync(item.Id);
                if (Issue == null) return BadRequest();
                Issue.IsActive = false;
                Issue.ModifyDate = DateTime.Now;
                Issue.ModifyBy = item.InputBy;
                await db.SaveChangesAsync();
                return Ok(Issue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
