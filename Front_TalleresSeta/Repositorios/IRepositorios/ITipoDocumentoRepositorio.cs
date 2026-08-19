using Front_TalleresSeta.Modelos.ModelosView;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface ITipoDocumentoRepositorio
    {
        Task<SelectList> ObtenerTipoDeDocumentosAsync(long idTaller);
    }
}