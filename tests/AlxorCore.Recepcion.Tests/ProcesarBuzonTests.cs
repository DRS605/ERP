using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Recepcion.Aplicacion;
using AlxorCore.Recepcion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Recepcion.Tests;

public sealed class ProcesarBuzonTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly IReloj Reloj = new RelojFijo();

    private sealed class BuzonFalso : IBuzonFacturas
    {
        public bool Configurado => true;

        public Task<IReadOnlyList<CorreoEntrante>> LeerNuevosAsync(CancellationToken ct = default)
        {
            var correo = new CorreoEntrante("proveedor@correo.com", "Facturas", Reloj.AhoraUtc, new[]
            {
                new AdjuntoCorreo("factura.pdf", "application/pdf", new byte[] { 1, 2, 3 }),
                new AdjuntoCorreo("firma.jpg", "image/jpeg", new byte[] { 4, 5 }), // se ignora (no es PDF)
            });
            return Task.FromResult<IReadOnlyList<CorreoEntrante>>(new[] { correo });
        }
    }

    private sealed class RepoFalso : IRepositorioFacturasRecibidas
    {
        public List<FacturaRecibida> Agregadas { get; } = new();

        public Task<FacturaRecibida?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Agregadas.Find(f => f.Id == id));

        public void Agregar(FacturaRecibida factura) => Agregadas.Add(factura);
    }

    private sealed class UnidadFalsa : IUnidadDeTrabajoRecepcion
    {
        public int Guardados { get; private set; }

        public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        {
            Guardados++;
            return Task.FromResult(0);
        }
    }

    [Fact]
    public async Task Procesar_da_de_alta_una_factura_por_cada_pdf()
    {
        var repo = new RepoFalso();
        var unidad = new UnidadFalsa();
        var caso = new ProcesarBuzon(new BuzonFalso(), repo, unidad, Reloj);

        var altas = await caso.EjecutarAsync(Empresa);

        altas.Should().Be(1); // solo el PDF, no el JPG
        repo.Agregadas.Should().ContainSingle();
        repo.Agregadas[0].Origen.Should().Be(OrigenRecepcion.Correo);
        repo.Agregadas[0].NombreArchivo.Should().Be("factura.pdf");
        repo.Agregadas[0].Estado.Should().Be(EstadoRecepcion.Recibida);
        unidad.Guardados.Should().Be(1);
    }
}
