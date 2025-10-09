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
    public class ReceiveController : ControllerBase
    {
        private readonly DataContext db;
        private StockService stockService = new StockService();    
      
        [HttpGet("Get")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Receive>>> GetAll()
        {
            try
            {
                var items = await db.Receives.Where(w => w.IsActive).OrderBy(o => o.Product).ToListAsync();
                //var items=dbc.DumpdataSapProductDatas.ToList();//ดึงข้อมูลจาก DbDataCenter2021Context

                if (items.Count == 0) return Ok(new List<Receive>());
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private ReceiveResponse GetDataResponse(ProductControll item)
        {
            var now = DateTime.Now.Date;
            var receive = db.Receives
                              .FirstOrDefault(r => r.IsActive && r.Product == item.Pd);

            var receivedate = receive != null ? receive.InputDate.Date : item.InputDate.Date;           

            var result = new ReceiveResponse()
            {
                WorkOrder = item.Wo,
               // StatusId = item.Status.Id,
                //StatusName = item.Status.Name,
                //Aging = (now - receivedate.Date).Days,
                //IsCheck = item.Status.Value != null && item.Status.Value.ToLower() != "complete",
                Product = product,
            };
            return result;
        }

        [HttpGet("GetGrouped")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<GroupedReceiveDto>>> GetGrouped()
        {
            try
            {
                var now = DateTime.Now; // หรือ DateTime.UtcNow

                var baseList = await db.WoControls.AsNoTracking()
                         .Where(w => w.IsActive && w.IsIssued == false /*&& w.Wo == 706034*/)
                         //.Where(w => w.IsActive && w.IsIssued == false)
                         .Select(w => new ReceiveResponse
                         {
                             WorkOrder = w.Wo,
                             StatusId = w.Status.Id,
                             StatusName = w.Status.Name,
                             Aging = EF.Functions.DateDiffDay(
                                         db.Receives
                                           .Where(r => r.IsActive && r.WorkOrder == w.Wo)
                                           .Select(r => (DateTime?)r.InputDate)
                                           .Min() ?? w.InputDate,
                                         now),
                             IsCheck = w.Status.Value != null && w.Status.Value.ToLower() != "complete",
                             Product = null // เติมทีหลัง
                         })
                         .ToListAsync();

                var woIds = baseList.Select(x => x.WorkOrder).Distinct().ToList();

                var prodMap = (await dbc.DumpdataSapProductionOrderHeaders.AsNoTracking()
                    .Where(x => x.MainOrder /*&& woIds.Contains((int)x.ProductionOrderId)*/)
                    .Select(x => new { Wo = (int)x.ProductionOrderId, x.ProductText })
                    .ToListAsync())
                    .ToDictionary(x => x.Wo, x => x.ProductText);

                foreach (var it in baseList)
                    it.Product = prodMap.TryGetValue(it.WorkOrder, out var p) ? p : null;


                return Ok(baseList);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }     

        [HttpGet("GetById/{workOder}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Receive>>> GetDataById(int workOder)
        {
            try
            {
                var item = await db.Receives.Where(w => w.WorkOrder == workOder).ToListAsync();

                if (item == null) return Ok(new Receive());
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
        public async Task<ActionResult<Receive>> Save(ReceiveVM item)
        {
            try
            {

                var location = await db.Locations.FindAsync(item.Location.Id);
                if (location == null) return BadRequest();

                // var area = await db.Areas.FindAsync(item.Area.Id);
                //if (area == null) return BadRequest();

                var group = await db.Groups.FindAsync(item.Group.Id);
                if (group == null) return BadRequest();

                var status = await db.Statuses.FindAsync(item.Status.Id);
                if (status == null) return BadRequest();

                var Receive = new Receive()

                {
                    EmployeeCode = item.EmployeeCode,
                    WorkOrder = item.WorkOrder,
                    Location = location,
                    Group = group,
                    Status = status,
                    Qty = item.Qty,
                    IsActive = true,
                    InputBy = item.InputBy,
                };
                db.Receives.Add(Receive);

                //check existingWo
                var wo = await db.WoControls
                .Where(w => w.IsActive && w.Wo == item.WorkOrder)
                .SingleOrDefaultAsync();

                bool isStatusChanged = wo?.Status.Id != status.Id;

                var result = new ReceiveResponse();
                if (wo == null || isStatusChanged)
                {
                    if (wo != null)
                        wo.IsActive = false;

                    var newWo = new WoControll
                    {
                        Wo = item.WorkOrder,
                        Status = status,
                        InputDate = DateTime.Now,
                        Qty = (wo?.Qty ?? 0) + item.Qty
                    };

                    db.WoControls.Add(newWo);
                    result = GetDataResponse(newWo);
                }
                else
                {

                    wo.Qty += item.Qty;
                    wo.IsIssued = wo.Qty == wo.IssueQty ? true : false;
                    result = GetDataResponse(wo);
                }

                //keep stock
                List<Models.Stock> stockList = new List<Models.Stock>();
                var date = DateTime.Now;
                stockService.addStockIn(date.Year, date.Month, location, group, item.WorkOrder, item.Qty, stockList);
                //update stock
                StockController stock = new StockController(db);
                await stock.StockMove(stockList);
                await db.SaveChangesAsync();

                return Ok(result);
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
                var Receive = await db.Receives.FindAsync(item.Id);
                if (Receive == null) return BadRequest();
                Receive.IsActive = false;
                Receive.ModifyDate = DateTime.Now;
                Receive.ModifyBy = item.InputBy;
                await db.SaveChangesAsync();
                return Ok(Receive);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
