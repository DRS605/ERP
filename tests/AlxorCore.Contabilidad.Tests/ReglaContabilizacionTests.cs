using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class ReglaContabilizacionTests
{
    private static readonly Guid Empresa = Guid.NewGuid();

    private sealed class RepoFake : IRepositorioReglasContabilizacion
    {
        private readonly List<ReglaContabilizacion> _reglas;
        public RepoFake(IEnumerable<ReglaContabilizacion> reglas) => _reglas = reglas.ToList();
        public void Agregar(ReglaContabilizacion regla) => _reglas.Add(regla);
        public Task<ReglaContabilizacion?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_reglas.FirstOrDefault(r => r.Id == id));
        public Task<IReadOnlyList<ReglaContabilizacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ReglaContabilizacion>>(_reglas.Where(r => r.EmpresaId == empresaId).ToList());
        public void Eliminar(ReglaContabilizacion regla) => _reglas.Remove(regla);
    }

    private static ReglaContabilizacion Regla(SentidoContable s, string? fam, string? tipo, string cuenta) =>
        ReglaContabilizacion.Crear(Empresa, s, fam, tipo, cuenta).Valor;

    [Fact]
    public void Una_regla_necesita_familia_o_tipo_y_una_cuenta()
    {
        ReglaContabilizacion.Crear(Empresa, SentidoContable.Venta, null, null, "705").EsFallo.Should().BeTrue();
        ReglaContabilizacion.Crear(Empresa, SentidoContable.Venta, "Merca", null, null).EsFallo.Should().BeTrue();
        ReglaContabilizacion.Crear(Empresa, SentidoContable.Venta, "Merca", null, "700").EsCorrecto.Should().BeTrue();
    }

    [Fact]
    public async Task Sin_reglas_devuelve_la_cuenta_generica()
    {
        var resolver = new ResolverCuentasReglas(new RepoFake(Array.Empty<ReglaContabilizacion>()));

        (await Resolver(resolver, SentidoContable.Venta, "X", "Y")).Should().Be(PlanBasico.CuentaVentas);
        (await Resolver(resolver, SentidoContable.Compra, null, "Y")).Should().Be(PlanBasico.CuentaCompras);
    }

    [Fact]
    public async Task Gana_la_regla_mas_especifica_familia_mas_tipo()
    {
        var resolver = new ResolverCuentasReglas(new RepoFake(new[]
        {
            Regla(SentidoContable.Venta, "Mercaderías", null, "700"),         // solo familia
            Regla(SentidoContable.Venta, null, "Intracomunitario", "701"),     // solo tipo
            Regla(SentidoContable.Venta, "Mercaderías", "Intracomunitario", "702"), // familia + tipo
        }));

        (await Resolver(resolver, SentidoContable.Venta, "Mercaderías", "Intracomunitario")).Should().Be("702");
        (await Resolver(resolver, SentidoContable.Venta, "Mercaderías", "Nacional")).Should().Be("700");
        (await Resolver(resolver, SentidoContable.Venta, "Servicios", "Intracomunitario")).Should().Be("701");
        (await Resolver(resolver, SentidoContable.Venta, "Servicios", "Nacional")).Should().Be(PlanBasico.CuentaVentas);
    }

    [Fact]
    public async Task Las_reglas_de_venta_y_compra_no_se_mezclan()
    {
        var resolver = new ResolverCuentasReglas(new RepoFake(new[]
        {
            Regla(SentidoContable.Compra, null, "Profesional", "623"),
        }));

        (await Resolver(resolver, SentidoContable.Compra, null, "Profesional")).Should().Be("623");
        (await Resolver(resolver, SentidoContable.Venta, null, "Profesional")).Should().Be(PlanBasico.CuentaVentas);
    }

    private static Task<string> Resolver(ResolverCuentasReglas r, SentidoContable s, string? fam, string? tipo) =>
        r.CuentaResultadoAsync(Empresa, s, fam, tipo);
}
