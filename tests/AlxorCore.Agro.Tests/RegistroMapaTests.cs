using System.Text;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

/// <summary>Lectura del fichero JSON del Registro Oficial de Productos Fitosanitarios del MAPA (extracto de un fichero real).</summary>
public sealed class RegistroMapaTests
{
    public const string Extracto = """
        {"Productos":[{"DATOSPRODUCTO":{"IdProducto":113941,"Num_Registro":"11179","Nombre":"MICROTHIOL SPECIAL DISPERSS","Titular":"UPL IBERIA, S.A.",
          "Fabricante":"UPL EUROPE LTD. (Warrington)","Formulado":"AZUFRE 80% [WG] P\/P","Fecha_Registro":"1974-12-13T00:00:00","Estado":"Vigente",
          "Fecha_Caducidad":"2027\/07\/31","Fecha_Cancelacion":"","Fecha_LimiteVenta":"","Condicionamiento":"Tratamiento fungicida preventivo."},
          "OTRASDENOMINACIONES":[{"Nombre_Origen":"COLPENN"}],"OTROSNOMBRES":[{"Nombre":"MICROTHIOL  SPECIAL DISPERSS"}],
          "COMPOSICION":[{"Nombre Sustancia":"AZUFRE","NombreUE":"Sulphur","Concentracion":8.000000000000000e+001,"DescripcionNota":"%"}],
          "USOS":[
            {"CodigoCultivo":"0101020200000000","Cultivo":"Olivo","CodigoAgente":"8.10.1.2","Agente":"Negrilla, fumagina, Capnodium elaeophilum","Dosis_Min":2.500000000000000e-001,
             "Dosis_Max":7.500000000000000e-001,"Unidad Medida dosis":"%","Plazo Seguridad":"NO PROCEDE","Volumen Caldo":"Uso profesional: 500-1333 l\/ha","Aplicaciones":"1-3",
             "IntervaloAplicaciones":"7 días","Bbch":"","CondicionamientoEspecifico":"Aplicar desde BBCH 71\rhasta BBCH 92.","TipoUsuario":"","Volumen_Min":0.0,"VolumenMax":0.0},
            {"CodigoCultivo":"0101020700000000","Cultivo":"Vid","CodigoAgente":"8.30.2.6","Agente":"Oídio de la vid, Erysiphe necator","Dosis_Min":2.500000000000000e-001,
             "Dosis_Max":8.000000000000000e-001,"Unidad Medida dosis":"%","Plazo Seguridad":"21","Aplicaciones":"1-8","IntervaloAplicaciones":"10 días","CondicionamientoEspecifico":""}
          ]}]}
        """;

    private static async Task<LecturaRegistroMapa> LeerAsync(string json) =>
        await ImportadorRegistroMapa.LeerAsync(new MemoryStream(Encoding.UTF8.GetBytes(json)), completa: true);

    [Fact]
    public async Task Lee_el_producto_su_composicion_y_sus_usos()
    {
        var lectura = await LeerAsync(Extracto);
        lectura.Avisos.Should().BeEmpty();
        lectura.Carga.Completa.Should().BeTrue();
        var p = lectura.Carga.Productos!.Single();
        p.Should().Match<DatosFitosanitario>(x => x.NumeroRegistro == "11179" && x.Nombre == "MICROTHIOL SPECIAL DISPERSS" && x.Titular == "UPL IBERIA, S.A."
            && x.Estado == EstadoFitosanitario.Autorizado && x.FechaCaducidad == new DateOnly(2027, 7, 31) && x.FechaCancelacion == null && x.Formulado == "AZUFRE 80% [WG] P/P");
        p.MateriasActivas.Should().ContainSingle().Which.Should().Be(new DatosMateriaActiva("AZUFRE", "80 %"));
        var olivo = p.Usos!.Single(u => u.Cultivo == "Olivo");
        olivo.Should().Match<DatosUsoFito>(u => u.Plaga == "Negrilla, fumagina, Capnodium elaeophilum" && u.DosisMinima == 0.25m && u.DosisMaxima == 0.75m
            && u.UnidadDosis == "%" && u.PlazoSeguridadDias == null && u.Aplicaciones == 3);
        olivo.Observaciones.Should().Be("Intervalo: 7 días. Aplicar desde BBCH 71 hasta BBCH 92.");
        p.Usos!.Single(u => u.Cultivo == "Vid").Should().Match<DatosUsoFito>(u => u.PlazoSeguridadDias == 21 && u.Aplicaciones == 8);
    }

    [Theory]
    [InlineData("Vigente", EstadoFitosanitario.Autorizado)]
    [InlineData("Cancelado", EstadoFitosanitario.Cancelado)]
    [InlineData("Caducado", EstadoFitosanitario.Caducado)]
    [InlineData("SUSPENDIDO", EstadoFitosanitario.Suspendido)]
    public void Traduce_el_estado(string texto, EstadoFitosanitario estado) => ImportadorRegistroMapa.Estado(texto).Should().Be(estado);

    [Theory]
    [InlineData("2027/07/31", 2027, 7, 31)]
    [InlineData("1974-12-13T00:00:00", 1974, 12, 13)]
    [InlineData("31/07/2027", 2027, 7, 31)]
    public void Lee_las_fechas(string texto, int a, int m, int d) => ImportadorRegistroMapa.Fecha(texto).Should().Be(new DateOnly(a, m, d));

    [Fact]
    public async Task Un_fichero_que_no_es_del_registro_se_rechaza()
    {
        var leer = () => LeerAsync("""{"Otra":[]}""");
        await leer.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task Una_segunda_carga_detecta_los_usos_retirados_y_los_cambios_de_fechas()
    {
        var reloj = new RelojFijo(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));
        var datos = (await LeerAsync(Extracto)).Carga.Productos!.Single();
        var producto = ProductoFitosanitario.Crear(Guid.NewGuid(), datos.Dominio(), reloj).Valor;
        var semana2 = (await LeerAsync(Extracto.Replace("2027\\/07\\/31", "2026\\/12\\/31", StringComparison.Ordinal)
            .Replace("\"Cultivo\":\"Vid\"", "\"Cultivo\":\"Viña\"", StringComparison.Ordinal))).Carga.Productos!.Single();
        producto.Aplicar(semana2.Dominio(), reloj, out var cambios).EsCorrecto.Should().BeTrue();
        cambios.Select(c => c.Tipo).Should().Contain([TipoCambioFito.Fechas, TipoCambioFito.UsoRetirado, TipoCambioFito.UsoNuevo]);
        cambios.Single(c => c.Tipo == TipoCambioFito.UsoRetirado).Detalle.Should().Contain("Vid / Oídio de la vid");
        producto.AplicableEl(new DateOnly(2027, 1, 15)).Should().BeFalse("caduca el 31/12/2026");
    }

    private sealed class RelojFijo(DateTimeOffset ahora) : IReloj
    {
        public DateTimeOffset AhoraUtc => ahora;
    }
}
