using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebAppEnvios.Migrations
{
    /// <inheritdoc />
    public partial class AddSucursalPagoExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "Paquetes",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Alto",
                table: "Paquetes",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Ancho",
                table: "Paquetes",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Paquetes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Largo",
                table: "Paquetes",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Paquetes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Costo",
                table: "Envios",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "NumeroSeguimiento",
                table: "Envios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Envios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Envios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Metodo = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comision = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.ForeignKey(
                        name: "FK_Pagos_Envios_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "Envios",
                        principalColumn: "EnvioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    SucursalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.SucursalId);
                });

            // Upsert estados usando MERGE para no violar FK con Envios existentes
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT EstadosEnvio ON;
                MERGE EstadosEnvio AS target
                USING (VALUES
                    (1, 'Pendiente',    'Orden registrada, en espera de recolección.'),
                    (2, 'Recolectado',  'Paquete recogido por mensajero.'),
                    (3, 'En Tránsito',  'Paquete en camino a destino.'),
                    (4, 'En Sucursal',  'Paquete disponible en sucursal destino.'),
                    (5, 'Entregado',    'Paquete entregado al destinatario.'),
                    (6, 'Cancelado',    'Envío cancelado por el cliente.'),
                    (7, 'No Entregado', 'Intento de entrega fallido.')
                ) AS source (EstadoId, NombreEstado, Descripcion)
                ON target.EstadoId = source.EstadoId
                WHEN MATCHED THEN UPDATE SET NombreEstado = source.NombreEstado, Descripcion = source.Descripcion
                WHEN NOT MATCHED THEN INSERT (EstadoId, NombreEstado, Descripcion) VALUES (source.EstadoId, source.NombreEstado, source.Descripcion);
                SET IDENTITY_INSERT EstadosEnvio OFF;
            ");

            migrationBuilder.InsertData(
                table: "Sucursales",
                columns: new[] { "SucursalId", "Activa", "Departamento", "Direccion", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "San Salvador", "Centro Histórico, San Salvador", "SmallBox San Salvador Central", "2222-0001" },
                    { 2, true, "Santa Ana", "4a Calle Ote, Santa Ana", "SmallBox Santa Ana", "2222-0002" },
                    { 3, true, "San Miguel", "Av. Roosevelt, San Miguel", "SmallBox San Miguel", "2222-0003" },
                    { 4, true, "La Libertad", "Calle Melchor Velásquez, Nueva San Salvador", "SmallBox La Libertad", "2222-0004" },
                    { 5, true, "Sonsonate", "Av. Morán, Sonsonate", "SmallBox Sonsonate", "2222-0005" },
                    { 6, true, "Usulután", "1a Av. Sur, Usulután", "SmallBox Usulután", "2222-0006" },
                    { 7, true, "Chalatenango", "Calle Principal, Chalatenango", "SmallBox Chalatenango", "2222-0007" },
                    { 8, true, "Cuscatlán", "Coja, Suchitoto", "SmallBox Cuscatlán", "2222-0008" },
                    { 9, true, "La Paz", "Centro, Zacatecoluca", "SmallBox La Paz", "2222-0009" },
                    { 10, true, "Cabañas", "Sensuntepeque Centro", "SmallBox Cabañas", "2222-0010" },
                    { 11, true, "San Vicente", "Parque Central, San Vicente", "SmallBox San Vicente", "2222-0011" },
                    { 12, true, "Ahuachapán", "2a Av. Norte, Ahuachapán", "SmallBox Ahuachapán", "2222-0012" },
                    { 13, true, "Morazán", "San Francisco Gotera Centro", "SmallBox Morazán", "2222-0013" },
                    { 14, true, "La Unión", "Puerto, La Unión", "SmallBox La Unión", "2222-0014" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Envios_SucursalId",
                table: "Envios",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_EnvioId",
                table: "Pagos",
                column: "EnvioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Envios_Sucursales_SucursalId",
                table: "Envios",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "SucursalId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Envios_Sucursales_SucursalId",
                table: "Envios");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Envios_SucursalId",
                table: "Envios");

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EstadosEnvio",
                keyColumn: "EstadoId",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Alto",
                table: "Paquetes");

            migrationBuilder.DropColumn(
                name: "Ancho",
                table: "Paquetes");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Paquetes");

            migrationBuilder.DropColumn(
                name: "Largo",
                table: "Paquetes");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Paquetes");

            migrationBuilder.DropColumn(
                name: "NumeroSeguimiento",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Envios");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "Paquetes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Costo",
                table: "Envios",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");
        }
    }
}
