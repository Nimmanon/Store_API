    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Stock.Context;
    using Stock.Models;
    using Stock.Models.ViewModels;
using Stock.Services;
using System;
using System.Runtime.ConstrainedExecution;

    namespace Stock.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class ReceiveController : ControllerBase
        {
            private readonly DataContext db;
        private StockService stockService = new StockService();
        public ReceiveController(DataContext db) => this.db = db;

        [HttpGet("Get")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Receive>>> GetAll()
        {
            try
            {
                var items = await db.Receives.Where(w => w.IsActive).OrderBy(o => o.Product)
                                    .OrderBy(o => o.Product)
                                    .ToListAsync();

                if (items.Count == 0) Ok(new List<Receive>());
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }


        //[HttpGet("GetById/{id}")]
        //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //    [ProducesResponseType(StatusCodes.Status404NotFound)]
        //    public async Task<ActionResult<List<Receive>>> GetDataById(int id)
        //    {
        //        try
        //        {
        //            var item = await db.Receives.Where(w => w.Id == id).SingleOrDefaultAsync();

        //            if (item == null) return Ok(new Receive());
        //            return Ok(item);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.ToString());
        //        }


        [HttpPost("Post")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Receive>> Save(Receive item)
        {
            try
            {
                var location = await db.Locations.FindAsync(item.Location.Id);
                if (location == null) return BadRequest();
             

                var model = new Receive()
                {
                    EmployeeCode = item.EmployeeCode,  
                    Product = item.Product,
                    Location = location,
                    Qty = item.Qty,
                    InputBy = item.InputBy,
                };

                db.Receives.Add(model);
                await db.SaveChangesAsync();

                //keep stock
                List<Models.StockWH> stockList = new List<Models.StockWH>();
                var date = DateTime.Now;
                stockService.addStockIn(date.Year, date.Month, location,  item.Product, item.Qty, stockList);
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

        [HttpPost("Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Receive>> Remove(ActionVM item)
        {
            try
            {
                var model = await db.Receives.FindAsync(item.Id);
                if (model == null) return BadRequest();

                model.IsActive = false;
                model.ModifyDate = DateTime.Now;
                model.ModifyBy = item.InputBy;

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
