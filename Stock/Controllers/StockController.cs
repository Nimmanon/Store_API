using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.Context;
using Stock.Models;
using Stock.Models.ViewModels;
using System.Text.RegularExpressions;

namespace Stock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly DataContext db;
        public StockController(DataContext db) => this.db = db;

        [HttpGet("GetOnHand/{item}/{location}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Receive>>> GetDataById(string item, int location)
        {
            try
            {
                var stock = await db.Stocks.Where(w => w.Year == DateTime.Now.Year && w.Month == DateTime.Now.Month && w.Product == item && w.Location.Id == location).SumAsync(s => s.BfQty + s.InQty - s.OutQty);


                return Ok(stock);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //[HttpGet("GetOnHand")]
        //public async Task<ActionResult<List<StockOnHandDto>>> GetAllOnHand()
        //{
        //    try
        //    {
        //        var now = DateTime.Now;

        //        var stocks = await db.Stocks
        //            .Where(w => w.Year == now.Year && w.Month == now.Month)
        //            .GroupBy(w => new
        //            {
        //                w.Product,
        //                LocationId = w.Location.Id,
        //                LocationName = w.Location.Name
        //            })
        //            .Select(g => new StockOnHandDto
        //            {
        //                Product = g.Key.Product,
        //                LocationId = g.Key.LocationId,
        //                LocationName = g.Key.LocationName,
        //                BfQty = g.Sum(x => x.BfQty),
        //                ReceiveQty = g.Sum(x => x.InQty),
        //                IssueQty = g.Sum(x => x.OutQty),
        //                OnHand = g.Sum(x => x.BfQty + x.InQty - x.OutQty)
        //            })
        //            .ToListAsync();

        //        return Ok(stocks);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        //    }
        //}

        [HttpGet("GetOnHand")]
        public async Task<ActionResult<List<StockOnHandDto>>> GetAllOnHand()
        {
            try
            {
                var now = DateTime.Now;

                var stocks = await db.Stocks
                    .Where(w => w.Year == now.Year && w.Month == now.Month)
                    .GroupBy(w => new
                    {
                        w.Product,
                        LocationId = w.Location.Id,
                        LocationName = w.Location.Name
                    })
                    // คำนวณยอดรวมก่อน
                    .Select(g => new
                    {
                        g.Key.Product,
                        g.Key.LocationId,
                        g.Key.LocationName,
                        BfQty = g.Sum(x => x.BfQty),
                        ReceiveQty = g.Sum(x => x.InQty),
                        IssueQty = g.Sum(x => x.OutQty),
                        OnHand = g.Sum(x => x.BfQty + x.InQty - x.OutQty)
                    })
                    // **กรองไม่เอา OnHand = 0**
                    .Where(x => x.OnHand != 0m)
                    // map กลับเป็น DTO
                    .Select(x => new StockOnHandDto
                    {
                        Product = x.Product,
                        LocationId = x.LocationId,
                        LocationName = x.LocationName,
                        BfQty = x.BfQty,
                        ReceiveQty = x.ReceiveQty,
                        IssueQty = x.IssueQty,
                        OnHand = x.OnHand
                    })
                    .ToListAsync();

                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        //อันที่เคยใช้
        [HttpPost("UpdateStockIn")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> StockIn(List<Models.StockWH> stockList)
        {
            try
            {

                //var locationList = db.Locations.Where(x => stockList.Select(s => s.Location.Id).ToList().Contains(x.Id)).ToList();
                //var areaList = db.Areas.Where(x => stockList.Select(s => s.Area.Id).ToList().Contains(x.Id)).ToList();
                //var unitList = db.Units.Where(x => stockList.Select(s => s.Unit.Id).ToList().Contains(x.Id)).ToList();
                //var groupList = db.Groups.Where(x => stockList.Select(s => s.Group.Id).ToList().Contains(x.Id)).ToList();

                var locationList = db.Locations.Where(x => x.IsActive).ToList();               

                foreach (var item in stockList)
                {
                    var stock = db.Stocks.Where(x =>
                                                    x.Year == item.Year &&
                                                    x.Month == item.Month &&
                                                    x.Location.Id == item.Location.Id &&
                                                    //x.Unit.Id == item.Unit.Id &&                                                   
                                                    x.Product == item.Product
                                                    ).SingleOrDefault();

                    if (stock != null)
                    {
                        stock.InQty += item.InQty;
                    }
                    else
                    {

                        var location = locationList.Where(x => x.Id == item.Location.Id).SingleOrDefault();
                        //var unit = unitList.Where(x => x.Id == item.Unit.Id).SingleOrDefault();                      
                        if (location != null)
                        //if (location != null && unit != null)
                        {
                            stock = new Models.StockWH
                            {
                                Year = item.Year,
                                Month = item.Month,
                                Location = location,
                                //Unit = unit,                               
                                Product = item.Product,
                                BfQty = 0,
                                InQty = item.InQty,
                                OutQty = 0,
                            };
                        }
                        db.Stocks.Add(stock);
                    }
                }
                db.ChangeTracker.AutoDetectChangesEnabled = true;
                db.ChangeTracker.DetectChanges();
                await StockBF_SetBasedAsync(stockList);
                await db.SaveChangesAsync();

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
        public async Task<bool> StockOut(List<Models.StockWH> stockList)
        {
            try
            {
                var locationList = db.Locations.Where(x => x.IsActive).ToList();            
               

                foreach (var item in stockList)
                {
                    var stock = db.Stocks.Where(x => x.Year == item.Year &&
                                                    x.Month == item.Month &&
                                                    x.Location.Id == item.Location.Id &&                                                   
                                                    x.Product == item.Product
                                                    ).SingleOrDefault();

                    if (stock != null)
                    {
                        stock.OutQty += item.OutQty;
                    }
                    else
                    {
                        var location = locationList.Where(x => x.Id == item.Location.Id).SingleOrDefault();                       

                        if (location != null)
                        //if (location != null && unit != null)
                        {
                            stock = new Models.StockWH
                            {
                                Year = item.Year,
                                Month = item.Month,
                                Location = location,
                                //Unit = unit,                               
                                Product = item.Product,
                                BfQty = 0,
                                InQty = 0,
                                OutQty = item.OutQty,
                            };
                        }
                        db.Stocks.Add(stock);
                    }
                }

                db.SaveChanges();
                //✅ เร็วกว่า: รวมซ้ำฝั่ง input ก่อน(ลดจำนวนรอบอัปเดต / แทรก)
                db.ChangeTracker.AutoDetectChangesEnabled = true;
                db.ChangeTracker.DetectChanges();
                await StockBF_SetBasedAsync(stockList);
                await db.SaveChangesAsync();
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
        public async Task<bool> StockMove(List<Models.StockWH> stockList)
        {
            try
            {
                var stockOutList = stockList.Where(x => x.OutQty != 0).ToList();
                await StockOut(stockOutList);

                var stockInList = stockList.Where(x => x.InQty != 0).ToList();
                await StockIn(stockInList);

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.ToString());
            }
        }

        private async Task StockBF_SetBasedAsync(List<Models.StockWH> stockList)
        {
            // กัน null
            if (stockList == null || stockList.Count == 0) return;

            // รวม period จาก input
            var periods = stockList
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new { g.Key.Year, g.Key.Month })
                .ToList();

            // ปิด AutoDetectChanges ระหว่าง bulk
            var detectFlag = db.ChangeTracker.AutoDetectChangesEnabled;
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                foreach (var p in periods)
                {
                    // period ถัดไป
                    int nyear = (p.Month == 12) ? p.Year + 1 : p.Year;
                    int nmonth = (p.Month == 12) ? 1 : p.Month + 1;

                    // ------- 1) ดึงข้อมูล period ปัจจุบันแบบ "ไม่ JOIN" -------
                    // ถ้ามีใน DB ใช้ของ DB; ถ้าไม่มี ใช้จาก stockList ที่ส่งเข้ามา
                    var currentDb = await db.Stocks
                        .Where(x => x.Year == p.Year && x.Month == p.Month)
                        .Select(x => new
                        {
                            x.Year,
                            x.Month,
                            LocId = EF.Property<int>(x, "LocationId"),                           
                            x.Product,
                            x.BfQty,
                            x.InQty,
                            x.OutQty
                        })
                        .ToListAsync();

                    IEnumerable<dynamic> current = currentDb;
                    if (currentDb.Count == 0)
                    {
                        // ใช้ของชุดที่เพิ่งอัปเดต (ยังไม่เซฟ) จาก input
                        current = stockList
                            .Where(x => x.Year == p.Year && x.Month == p.Month)
                            .Select(x => new
                            {
                                x.Year,
                                x.Month,
                                LocId = x.Location.Id,                               
                                x.Product,
                                x.BfQty,
                                x.InQty,
                                x.OutQty
                            })
                            .ToList();
                    }

                    // ------- 2) คำนวณ "carry" (B/F ของเดือนถัดไป) แบบรวมซ้ำ -------
                    var carries = current
                        .GroupBy(x => new { x.Year, x.Month, x.LocId, x.GrpId, x.Product })
                        .Select(g => new
                        {
                            LocId = g.Key.LocId,                           
                            Product = g.Key.Product,
                            Carry = g.Sum(z => z.BfQty + z.InQty - z.OutQty)
                        })
                        .Where(a => a.Carry != 0) // ไม่มีความหมายไม่ต้องสร้าง/อัปเดต
                        .ToList();

                    if (carries.Count == 0) continue;

                    // ------- 3) ดึง stocks ของ period ถัดไป "เฉพาะคีย์ที่เกี่ยวข้อง" -------
                    var locIds = carries.Select(c => c.LocId).Distinct().ToList();                    
                    var pos = carries.Select(c => c.Product).Distinct().ToList();

                    var nextStocks = await db.Stocks
                        .Where(x => x.Year == nyear
                                 && x.Month == nmonth
                                 && locIds.Contains(EF.Property<int>(x, "LocationId"))                                
                                 && pos.Contains(x.Product))
                        .ToListAsync();

                    string Key(int l,  string prod) => $"{l}|{prod}";

                    var mapNext = nextStocks.ToDictionary(
                        k => Key(EF.Property<int>(k, "LocationId"), k.Product),
                        v => v);

                    // เตรียม stub entities เพื่อหลีกเลี่ยง SELECT Nav
                    var locStubs = new Dictionary<int, Location>();                    

                    // ------- 4) อัปเดต/สร้าง B/F ใน period ถัดไป แบบไม่ JOIN -------
                    foreach (var c in carries)
                    {
                        var key = Key(c.LocId,  c.Product);
                        if (mapNext.TryGetValue(key, out var next))
                        {
                            next.BfQty += c.Carry; // update ยอด BF กลางเดือนถัดไป
                        }
                        else
                        {

                            Location stubLoc;
                            if (!locStubs.TryGetValue(c.LocId, out stubLoc))
                            {
                                stubLoc = new Location { Id = c.LocId };
                                db.Attach(stubLoc);
                                locStubs[c.LocId] = stubLoc;
                            }

                            var newNext = new Models.StockWH
                            {
                                Year = nyear,
                                Month = nmonth,
                                Location = stubLoc,   // ตั้ง nav โดยไม่โหลดจริง                               
                                Product = c.Product,
                                BfQty = c.Carry,
                                InQty = 0,
                                OutQty = 0
                            };

                            db.Stocks.Add(newNext);
                            mapNext[key] = newNext;
                        }
                    }
                }
            }
            finally
            {
                // เปิด detect changes กลับ
                db.ChangeTracker.AutoDetectChangesEnabled = detectFlag;
            }
        }

        // แนะนำให้เรียกใช้งานเมธอดนี้โดยห่อด้วย:
        // db.ChangeTracker.AutoDetectChangesEnabled = false;
        // ... เรียก UpdateStock(...) หลาย ๆ ครั้ง ...
        // db.ChangeTracker.AutoDetectChangesEnabled = true; db.ChangeTracker.DetectChanges();

        private void UpdateStock(
            int year, int month, int nyear, int nmonth,
            Location location, string product,
            List<StockWH> stockList,
            decimal bfqty, decimal inqty, decimal outqty)
        {
            // 1) เตรียม map คีย์ -> Stock (สร้างครั้งแรกแล้ว reuse ในครั้งถัด ๆ ไป)
            //    ถ้าคุณเรียกเมธอดนี้หลายรอบในลูปใหญ่ แนะนำให้ย้าย map ออกไปสร้าง "ข้างนอก" แล้วส่งเข้ามาเป็นพารามิเตอร์แทน
            string Key(int y, int m, int locId, string prod) => $"{y}|{m}|{locId}";

            // สร้าง map จาก stockList ถ้ายังไม่มี (ป้องกันค่า duplicate; ใช้ตัวแรกเป็นหลัก)
            // NOTE: ถ้าสต็อกเยอะมาก ๆ ให้สร้างและถือ map จากภายนอก จะเร็วกว่า
            var map = new Dictionary<string, StockWH>(capacity: stockList.Count * 2);
            foreach (var s in stockList)
            {
                var k = Key(s.Year, s.Month, s.Location.Id, s.Product);
                if (!map.ContainsKey(k)) map.Add(k, s);
            }

            // 2) current period
            var curKey = Key(year, month, location.Id, product);
            if (!map.TryGetValue(curKey, out var stock))
            {
                stock = new StockWH
                {
                    Year = year,
                    Month = month,
                    Location = location,   // ใช้ nav เดิม ไม่ยิง SELECT เพิ่ม                  
                    Product = product,
                    BfQty = 0,
                    InQty = 0,
                    OutQty = 0
                };
                db.Stocks.Add(stock);
                stockList.Add(stock);
                map[curKey] = stock;
            }

            // อัปเดตค่าปัจจุบัน (คง logic เดิมของคุณ = set ค่า ไม่ใช่สะสม)
            stock.BfQty = bfqty;
            stock.InQty = inqty;
            stock.OutQty = outqty;

            // 3) next period (BF ของงวดถัดไป = BF + IN - OUT ของปัจจุบัน)
            var carry = (stock.BfQty + stock.InQty) - stock.OutQty;

            var nextKey = Key(nyear, nmonth, location.Id,  product);
            if (!map.TryGetValue(nextKey, out var nstock))
            {
                nstock = new StockWH
                {
                    Year = nyear,
                    Month = nmonth,
                    Location = location,                   
                    Product = product,
                    BfQty = 0,
                    InQty = 0,
                    OutQty = 0
                };
                db.Stocks.Add(nstock);
                stockList.Add(nstock);
                map[nextKey] = nstock;
            }

            // คง logic เดิม: งวดถัดไป set ค่าแบบรีเซ็ต IN/OUT เป็น 0
            nstock.BfQty = carry;
            nstock.InQty = 0;
            nstock.OutQty = 0;
        }


        private List<Models.StockWH> GetStockData(StockRequest item)
        {
            try
            {
                var stockAllList = new List<StockResponse>();

                //get bf stock
                var current = new DateTime(item.Year, item.Month, 1);
                int year = current.Year;
                int month = current.Month;

                int bfyear = current.AddMonths(-1).Year;
                int bfmonth = current.AddMonths(-1).Month;

                //bf
                var bfstock = db.Stocks.Where(x => x.Year == bfyear && x.Month == bfmonth).ToList();

                if (item?.Location != null)
                {
                    bfstock = bfstock.Where(x => x.Location.Id == item?.Location.Id).ToList();
                }              

                if (item?.Product != null)
                {
                    bfstock = bfstock.Where(x => x.Product == item.Product).ToList();
                   
                }

                bfstock.ForEach(x =>
                {
                    var st = new StockResponse();
                    st.Year = year;
                    st.Month = month;
                    DateTime dt = new DateTime(year, month, 1, 0, 0, 0);
                    st.TransactionDate = dt;
                    st.Location = x.Location;                    
                    st.Product = x.Product;
                    st.BfQty = (x.BfQty + x.InQty) - x.OutQty;
                    st.InQty = 0;
                    st.OutQty = 0;
                    stockAllList.Add(st);
                });

                #region get in with receive
                var receive = db.Receives.Where(x => x.IsActive == true
                                                        && x.InputDate.Date.Year == year
                                                        && x.InputDate.Date.Month == month).ToList();

                if (item?.Location != null)
                {
                    receive = receive.Where(x => x.Location.Id == item?.Location.Id).ToList();
                }              

                if (item?.Product != null)
                {
                    receive = receive.Where(x => x.Product == item?.Product).ToList();
                }

                receive.ForEach(x =>
                {
                    var st = new StockResponse();
                    st.TransactionDate = x.InputDate;
                    st.Location = x.Location;                   
                    st.Product = x.Product;
                    st.BfQty = 0;
                    st.InQty = x.Qty;
                    st.OutQty = 0;
                    stockAllList.Add(st);
                });
                #endregion                

                #region get out with issue
                var issue = db.Issues.Where(x => x.IsActive == true
                                                    && x.InputDate.Date.Year == year
                                                    && x.InputDate.Date.Month == month).ToList();

                if (item?.Location != null)
                {
                    issue = issue.Where(x => x.Location.Id == item?.Location.Id).ToList();
                }               

                if (item?.Product != null)
                {
                    issue = issue.Where(x => x.Product == item?.Product).ToList();
                }

                issue.ForEach(x =>
                {
                    var st = new StockResponse();
                    st.TransactionDate = x.InputDate;
                    st.Location = x.Location;                   
                    st.Product = x.Product;
                    st.BfQty = 0;
                    st.InQty = 0;
                    st.OutQty = x.Qty;
                    stockAllList.Add(st);
                });
                #endregion

                //summary data
                var stockall = stockAllList.GroupBy(g => new
                {
                    Year = g.TransactionDate.Year,
                    Month = g.TransactionDate.Month,
                    Location = g.Location,
                    Product = g.Product
                }).Select(x => new StockWH()
                {
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Location = x.Key.Location,                    
                    Product = x.Key.Product,
                    BfQty = x.Sum(u => u.BfQty),
                    InQty = x.Sum(u => u.InQty),
                    OutQty = x.Sum(u => u.OutQty)
                }).ToList();

                return stockall;
            }
            catch (Exception ex)
            {
                return new List<Models.StockWH>();
            }
        }

        [HttpGet("BroughtForward")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> BroughtForward()
        {
            try
            {
                //get Year,Month Lastest Calculate Stock
                var period = await db.Settings.Where(x => x.IsActive == true && x.Prefix.ToLower() == "calstock").SingleOrDefaultAsync();
                if (period?.Value1 == DateTime.Now.Year && period?.Value2 == DateTime.Now.Month) return Ok();

                var cur = DateTime.Now;
                var bf = cur.AddMonths(-1);
                DateTime start = new DateTime(bf.Year, bf.Month, 1, 0, 0, 0);
                DateTime finish = new DateTime(cur.Year, cur.Month, DateTime.DaysInMonth(cur.Year, cur.Month), 0, 0, 0);

                while (start <= finish)
                {
                    //summary data
                    var c = new StockRequest();
                    c.Year = start.Year;
                    c.Month = start.Month;
                    var stockall = GetStockData(c);

                    var current = new DateTime(start.Year, start.Month, 1);
                    int year = current.Year;
                    int month = current.Month;

                    int nyear = current.AddMonths(1).Year;
                    int nmonth = current.AddMonths(1).Month;

                    //update stock   
                    var stockList = db.Stocks.Where(x => (x.Year == year && x.Month == month) || (x.Year == nyear && x.Month == nmonth))
                                            .OrderBy(o => o.Location.Name)                                            
                                            .ThenBy(o => o.Product)
                                            .ThenBy(o => o.Year)
                                            .ThenBy(o => o.Month)
                                            .ToList();

                    //clear stock current preiod
                    stockList.ForEach(x =>
                    {
                        x.BfQty = 0;
                        x.InQty = 0;
                        x.OutQty = 0;
                    });

                    var locationList = db.Locations.ToList();                   

                    stockall.ForEach(async st =>
                    {
                        var location = locationList.Where(x => x.Id == st.Location.Id).SingleOrDefault();
                        if (location == null) BadRequest();                      

                        UpdateStock(year, month, nyear, nmonth, location,  st.Product, stockList,
                                    st.BfQty, st.InQty, st.OutQty);
                    });

                    await db.SaveChangesAsync();

                    start = start.AddMonths(1);
                }

                //update Year,Month last Calculate Stock in Setting
                period.Value1 = cur.Year;
                period.Value2 = cur.Month;
                await db.SaveChangesAsync();

                return Ok("Stock calculation is successful.");
            }
            catch (Exception ex)
            {
                return BadRequest("Stock calculation is unsuccess.");
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("CalculateStock")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> StockCalculate(StockRequest item)
        {
            try
            {
                DateTime start = new DateTime(item.Year, item.Month, 1, 0, 0, 0);
                DateTime finish = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 0, 0, 0);

                while (start <= finish)
                {
                    //summary data
                    var c = new StockRequest();
                    c.Year = start.Year;
                    c.Month = start.Month;
                    c.Location = item.Location;                  
                    c.Product = item.Product;
                    var stockall = GetStockData(c);

                    var current = new DateTime(start.Year, start.Month, 1);
                    int year = current.Year;
                    int month = current.Month;

                    int bfyear = current.AddMonths(-1).Year;
                    int bfmonth = current.AddMonths(-1).Month;

                    int nyear = current.AddMonths(1).Year;
                    int nmonth = current.AddMonths(1).Month;

                    //update stock   
                    var stockList = db.Stocks.Where(x => (x.Year == year && x.Month == month) || (x.Year == nyear && x.Month == nmonth))
                                            .OrderBy(o => o.Location.Name)                                            
                                            .ThenBy(o => o.Product)
                                            .ThenBy(o => o.Year)
                                            .ThenBy(o => o.Month)
                                            .ToList();



                    if (item?.Location != null)
                    {
                        stockList = stockList.Where(x => x.Location.Id == item.Location.Id).ToList();
                    }                    

                    if (item?.Product != null)
                    {
                        stockList = stockList.Where(x => x.Product == item.Product).ToList();
                    }

                    //clear stock current preiod
                    stockList.ForEach(x =>
                    {
                        x.BfQty = 0;
                        x.InQty = 0;
                        x.OutQty = 0;
                    });

                    var locationList = db.Locations.ToList();                    

                    stockall.ForEach(async st =>
                    {
                        var location = locationList.Where(x => x.Id == st.Location.Id).SingleOrDefault();
                        if (location == null) BadRequest();                       

                        UpdateStock(year, month, nyear, nmonth, location,  st.Product, stockList,
                                    st.BfQty, st.InQty, st.OutQty);
                    });

                    await db.SaveChangesAsync();

                    start = start.AddMonths(1);
                }
                return Ok("Stock calculation is successful.");
            }
            catch (Exception ex)
            {
                return BadRequest("Stock calculation is unsuccess.");
                throw new Exception(ex.ToString());
            }
        }


    }
}
