using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value3 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value4 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value5 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Actual1 = table.Column<bool>(type: "bit", nullable: false),
                    Actual2 = table.Column<bool>(type: "bit", nullable: false),
                    Actual3 = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
