using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Stock.Context
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            // กรณีรันจากโฟลเดอร์อื่น ให้ชี้มาที่โปรเจกต์ Stock โดยตรง
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "D:\\Github\\Store\\API\\Stock");
            if (!Directory.Exists(basePath)) basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("DataContext")
                ?? "Server=192.168.0.88;Database=Stock_WH;Persist Security Info=True;User ID=sa;Password=tlp72534%;MultipleActiveResultSets=True;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer(cs)
                .Options;

            return new DataContext(options);
        }
    }
}
