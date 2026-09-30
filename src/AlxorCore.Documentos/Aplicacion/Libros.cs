namespace AlxorCore.Documentos.Aplicacion;

/// <summary>Tabla de un libro: columnas (con las numéricas alineadas a la derecha) y filas; una fila en negrita es un total.</summary>
public sealed record TablaLibro(string? Titulo, IReadOnlyList<string> Columnas, IReadOnlyList<bool> Numericas, IReadOnlyList<FilaLibro> Filas,
    IReadOnlyList<float>? Anchos = null);

public sealed record FilaLibro(IReadOnlyList<string> Celdas, bool Negrita = false);

/// <summary>Sección de un libro: empieza en página nueva, con su título, un texto y sus tablas.</summary>
public sealed record SeccionLibro(string Titulo, string? Texto, IReadOnlyList<TablaLibro> Tablas);

/// <summary>
/// Libro oficial para legalizar (diario, inventarios y cuentas anuales, actas…): cabecera con la empresa en cada hoja,
/// páginas numeradas correlativas y apaisado si se pide.
/// </summary>
public sealed record LibroImpreso(string Titulo, string Empresa, string Nif, string Periodo, IReadOnlyList<SeccionLibro> Secciones, bool Apaisado = false);

public interface IGeneradorPdfLibro
{
    byte[] Generar(LibroImpreso libro);
}
