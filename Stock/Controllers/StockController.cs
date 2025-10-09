using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stock.Context;
using Stock.Models;
using System.Text.RegularExpressions;

namespace Stock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly DataContext db;
        public StockController(DataContext db) => this.db = db;
       

        //อันที่เคยใช้ ที่ทำให้ช้ามากกกกกกกกกกกก
        [HttpPost("UpdateStockIn")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> StockIn(List<Models.StockWH> stockList)
        {
            try
            {

                var locationList = db.Locations.Where(x => x.IsActive).ToList();
                //var unitList = db.Units.Where(x => x.IsActive).ToList();
                //var groupList = db.Groups.Where(x => x.IsActive).ToList();

                foreach (var item in stockList)
                {
                    var stock = db.Stocks.Where(x =>
                                                    x.Year == item.Year &&
                                                    x.Month == item.Month &&
                                                    x.Location.Id == item.Location.Id &&
                                                    x.Product == item.Product
                                                    //x.Unit.Id == item.Unit.Id &&
                                                    //x.Group.Id == item.Group.Id &&
                                                    //x.WorkOrder == item.WorkOrder
                                                    ).SingleOrDefault();

                    if (stock != null)
                    {
                        stock.InQty += item.InQty;
                    }
                    else
                    {

                        var location = locationList.Where(x => x.Id == item.Location.Id).SingleOrDefault();
                        //var unit = unitList.Where(x => x.Id == item.Unit.Id).SingleOrDefault();
                        var group = groupList.Where(x => x.Id == item.Group.Id).SingleOrDefault();
                        if (location != null)
                        //if (location != null && unit != null)
                        {
                            stock = new Models.Stock
                            {
                                Year = item.Year,
                                Month = item.Month,
                                Location = location,
                                //Unit = unit,
                                Group = group,
                                WorkOrder = item.WorkOrder,
                                BfQty = 0,
                                InQty = item.InQty,
                                OutQty = 0,
                            };
                        }
                        db.Stocks.Add(stock);
                    }
                }

                //update bf
                StockBF(stockList);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.ToString());
            }
        }


        [HttpPost("UpdateStockOut")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> StockOut(List<Models.Stock> stockList)
        {
            try
            {
                var locationList = db.Locations.Where(x => x.IsActive).ToList();
                var unitList = db.Units.Where(x => x.IsActive).ToList();
                var groupList = db.Groups.Where(x => x.IsActive).ToList();

                foreach (var item in stockList)
                {
                    var stock = db.Stocks.Where(x => x.Year == item.Year &&
                                                    x.Month == item.Month &&
                                                    x.Location.Id == item.Location.Id &&
                                                    //x.Unit.Id == item.Unit.Id &&
                                                    x.Group.Id == item.Group.Id &&
                                                    x.WorkOrder == item.WorkOrder
                                                    ).SingleOrDefault();

                    if (stock != null)
                    {
                        stock.OutQty += item.OutQty;
                    }
                    else
                    {
                        var location = locationList.Where(x => x.Id == item.Location.Id).SingleOrDefault();
                        //var unit = unitList.Where(x => x.Id == item.Unit.Id).SingleOrDefault();
                        var group = groupList.Where(x => x.Id == item.Group.Id).SingleOrDefault();

                        if (location != null)
                        //if (location != null && unit != null)
                        {
                            stock = new Models.Stock
                            {
                                Year = item.Year,
                                Month = item.Month,
                                Location = location,
                                //Unit = unit,
                                Group = group,
                                WorkOrder = item.WorkOrder,
                                BfQty = 0,
                                InQty = 0,
                                OutQty = item.OutQty,
                            };
                        }
                        db.Stocks.Add(stock);
                    }
                }

                //update bf
                StockBF(stockList);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("UpdateStockMove")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> StockMove(List<Models.Stock> stockList)
        {
            try
            {
                var stockOutList = stockList.Where(x => x.OutQty != 0).ToList();
                StockOut(stockOutList);

                var stockInList = stockList.Where(x => x.InQty != 0).ToList();
                StockIn(stockInList);

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.ToString());
            }
        }

        private async void StockBF(List<Models.Stock> stockList)
        {
            try
            {
                var periodList = stockList.GroupBy(x => new { Year = x.Year, Month = x.Month }).ToList();
                foreach (var p in periodList)
                {
                    //get next period
                    int nyear = p.Key.Month + 1 > 12 ? p.Key.Year + 1 : p.Key.Year;
                    int nmonth = p.Key.Month + 1 > 12 ? 1 : p.Key.Month + 1;

                    //get data stock
                    var stockNextList = db.Stocks.Where(x => x.Year == nyear && x.Month == nmonth).ToList();

                    var dataList = db.Stocks.Where(x => x.Year == p.Key.Year
                                                         && x.Month == p.Key.Month
                                                         )
                                    .Include(x => x.Location)
                                    //.Include(x => x.Unit)
                                    .Include(x => x.Group)
                                    .ToList();

                    if (dataList?.Count() == 0)
                    {
                        dataList = stockList.Where(x => x.Year == p.Key.Year && x.Month == p.Key.Month).ToList();
                    }

                    var locationList = db.Locations.Where(x => x.IsActive).ToList();
                    var unitList = db.Units.Where(x => x.IsActive).ToList();
                    var groupList = db.Groups.Where(x => x.IsActive).ToList();


                    foreach (var item in dataList)
                    {
                        var stock = stockNextList.Where(x =>
                                                   x.Year == item.Year &&
                                                    x.Month == item.Month &&
                                                    x.Location.Id == item.Location.Id &&
                                                    //x.Unit.Id == item.Unit.Id &&
                                                    x.Group.Id == item.Group.Id &&
                                                    x.WorkOrder == item.WorkOrder
                                                    ).SingleOrDefault();

                        if (stock != null)
                        {
                            stock.BfQty += item.BfQty + item.InQty - item.OutQty;
                        }
                        else
                        {
                            var location = locationList.Where(x => x.Id == item.Location.Id).SingleOrDefault();
                            //var unit = unitList.Where(x => x.Id == item.Unit.Id).SingleOrDefault();
                            var group = groupList.Where(x => x.Id == item.Group.Id).SingleOrDefault();

                            if (location != null)
                            //if (location != null && unit != null)
                            {
                                stock = new Models.Stock
                                {
                                    Year = nyear,
                                    Month = nmonth,
                                    Location = location,
                                    //Unit = unit,
                                    Group = group,
                                    WorkOrder = item.WorkOrder,
                                    BfQty = item.BfQty + item.InQty - item.OutQty,
                                    InQty = 0,
                                    OutQty = 0,
                                };
                            }

                            db.Stocks.Add(stock);
                            stockList.Add(stock);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private async void UpdateStock(int year, int month, int nyear, int nmonth, Location location, Group group, int WorkOrder, List<Stock> stockList, decimal bfqty, decimal inqty, decimal outqty)
        {
            try
            {
                //current Period
                var stock = stockList.Where(x =>
                                            x.Year == year
                                            && x.Month == month
                                            && x.Location.Id == location.Id
                                            //&& x.Unit.Id == unit.Id
                                            && x.Group.Id == group.Id
                                            && x.WorkOrder == WorkOrder)
                    .FirstOrDefault();
                if (stock != null)
                {
                    //update stock
                    stock.BfQty = bfqty;
                    stock.InQty = inqty;
                    stock.OutQty = outqty;
                }
                else
                {
                    //add new stock
                    stock = new Models.Stock
                    {
                        Year = year,
                        Month = month,
                        Location = location,
                        //Unit = unit,
                        Group = group,
                        WorkOrder = WorkOrder,
                        BfQty = bfqty,
                        InQty = inqty,
                        OutQty = outqty,
                    };
                    db.Stocks.Add(stock);
                    stockList.Add(stock);
                }

                //next Period
                var nQty = (stock.BfQty + stock.InQty) - stock.OutQty;

                var nstock = stockList.Where(x =>
                                              x.Year == nyear
                                            && x.Month == nmonth
                                            && x.Location.Id == location.Id
                                            //&& x.Unit.Id == unit.Id
                                            && x.Group.Id == group.Id
                                            && x.WorkOrder == WorkOrder).FirstOrDefault();
                if (nstock != null)
                {
                    //update next stock
                    nstock.BfQty = nQty;
                    nstock.InQty = 0;
                    nstock.OutQty = 0;
                }
                else
                {
                    //add new next stock  
                    nstock = new Models.Stock
                    {
                        Year = nyear,
                        Month = nmonth,
                        Location = location,
                        ///Unit = unit,
                        Group = group,
                        WorkOrder = WorkOrder,
                        BfQty = nQty,
                        InQty = 0,
                        OutQty = 0,
                    };
                    db.Stocks.Add(nstock);
                    stockList.Add(nstock);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

      
    }
}
