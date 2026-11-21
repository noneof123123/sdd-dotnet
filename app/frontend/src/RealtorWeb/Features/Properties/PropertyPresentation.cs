namespace RealtorWeb.Features.Properties;

public static class PropertyPresentation
{
    public const string ErrorMessage = "No se pudo cargar la información. Inténtalo de nuevo.";
    public const string EmptyMessage = "No hay propiedades disponibles en esta página.";
    public const string NotFoundMessage = "No encontramos esta propiedad.";
    public const string LoadingCatalogMessage = "Cargando propiedades…";
    public const string LoadingDetailsMessage = "Cargando propiedad…";
    public const string PaginationNavLabel = "Paginación del catálogo";
    public const string PreviousControlText = "Anterior";
    public const string NextControlText = "Siguiente";
    public const string PreviousDisabledLabel = "Página anterior no disponible";
    public const string NextDisabledLabel = "Página siguiente no disponible";

    public static PresentationState CatalogState(bool isLoading, bool succeeded, bool hasItems)
    {
        if (isLoading)
        {
            return PresentationState.Cargando;
        }

        if (!succeeded)
        {
            return PresentationState.Error;
        }

        return hasItems ? PresentationState.Contenido : PresentationState.Vacio;
    }

    public static PresentationState DetailsState(bool isLoading, bool succeeded, bool notFound)
    {
        if (isLoading)
        {
            return PresentationState.Cargando;
        }

        if (notFound)
        {
            return PresentationState.NoEncontrado;
        }

        return succeeded ? PresentationState.Contenido : PresentationState.Error;
    }

    public static string MessageFor(PresentationState state, bool isDetails = false) => state switch
    {
        PresentationState.Cargando => isDetails ? LoadingDetailsMessage : LoadingCatalogMessage,
        PresentationState.Vacio => EmptyMessage,
        PresentationState.Error => ErrorMessage,
        PresentationState.NoEncontrado => NotFoundMessage,
        _ => string.Empty
    };
}
