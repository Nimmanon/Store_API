using Stock.Models;
using System.Text.RegularExpressions;

namespace Stock.Services
{
    public class StockService
    {
        public bool addStockIn(int year, int month, Location location, string product, decimal qty, List<Models.StockWH> stockList = null!)
        {
            try
            {
                //keep Stock      
                var stockExits = stockList.Where(x =>
                                            x.Year == year &&
                                            x.Month == month &&
                                            x.Location.Id == location.Id &&
                                            // x.Area.Id == area.Id &&
                                            //x.Unit.Id == unit.Id &&
                                            //x.Group.Id == group.Id &&
                                            //x.WorkOrder == workorder &&
                                            x.Product == product
                                            ).SingleOrDefault();
                if (stockExits != null)
                {
                    stockExits.InQty += qty;
                }
                else
                {
                    var stock = new Models.StockWH();
                    stock.Year = year;
                    stock.Month = month;
                    stock.Location = location;
                    // stock.Area = area;
                    //stock.Unit = unit;
                    //stock.Group = group;
                    //stock.WorkOrder = workorder;
                    stock.Product = product;
                    stock.InQty = qty;
                    stock.OutQty = 0;
                    stockList.Add(stock);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool addStockOut(int year, int month, Location location, string product, decimal qty, List<Models.StockWH> stockList = null!)
        {
            {
                try
                {
                    //keep Stock      
                    var stockExits = stockList.Where(x =>
                                                x.Year == year &&
                                                x.Month == month &&
                                                x.Location.Id == location.Id &&
                                                // x.Area.Id == area.Id &&
                                                //x.Unit.Id == unit.Id &&
                                                //x.Group.Id == group.Id &&
                                                //x.WorkOrder == workorder
                                                x.Product == product
                                                ).SingleOrDefault();
                    if (stockExits != null)
                    {
                        stockExits.OutQty += qty;
                    }
                    else
                    {
                        var stock = new Models.StockWH();
                        stock.Year = year;
                        stock.Month = month;
                        stock.Location = location;
                        //stock.Area = area;
                        // stock.Unit = unit;
                        //stock.Group = group;
                        //stock.WorkOrder = workorder;
                        stock.Product = product;
                        stock.InQty = 0;
                        stock.OutQty = qty;
                        stockList.Add(stock);
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        public bool removeStockIn(int year, int month, Location location,  string product, decimal qty, List<Models.StockWH> stockList = null!)
        {
            try
            {
                //keep Stock      
                var stockExits = stockList.Where(x =>
                                               x.Year == year &&
                                                x.Month == month &&
                                                x.Location.Id == location.Id &&
                                                // x.Area.Id == area.Id &&
                                                //x.Unit.Id == unit.Id &&
                                                //x.Group.Id == group.Id &&
                                                //x.WorkOrder == workorder
                                                x.Product == product
                                            ).SingleOrDefault();
                if (stockExits != null)
                {
                    stockExits.InQty -= qty;
                }
                else
                {
                    var stock = new Models.StockWH();
                    stock.Year = year;
                    stock.Month = month;
                    stock.Location = location;
                    //stock.Area = area;
                    //stock.Unit = unit;
                    //stock.Group = group;
                    //stock.WorkOrder = workorder;
                    stock.Product = product;
                    stock.InQty -= qty;
                    stock.OutQty = 0;
                    stockList.Add(stock);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool removeStockOut(int year, int month, Location location,  string product, decimal qty, List<Models.StockWH> stockList = null!)
        {
            try
            {
                //keep Stock      
                var stockExits = stockList.Where(x =>
                                           x.Year == year &&
                                                x.Month == month &&
                                                x.Location.Id == location.Id &&
                                                //x.Area.Id == area.Id &&
                                                //x.Unit.Id == unit.Id &&
                                                //x.Group.Id == group.Id &&
                                                //x.WorkOrder == workorder
                                                x.Product == product
                                            ).SingleOrDefault();
                if (stockExits != null)
                {
                    stockExits.OutQty -= qty;
                }
                else
                {
                    var stock = new Models.StockWH();
                    stock.Year = year;
                    stock.Month = month;
                    stock.Location = location;
                    //stock.Area = area;
                    //stock.Unit = unit;
                    //stock.Group = group;
                    //stock.WorkOrder = workorder;
                    stock.Product = product;
                    stock.InQty = 0;
                    stock.OutQty -= qty;
                    stockList.Add(stock);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
