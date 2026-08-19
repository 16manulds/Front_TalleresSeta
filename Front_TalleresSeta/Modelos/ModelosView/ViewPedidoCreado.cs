using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public partial class ViewPedidoCreado
    {
        public long PedidoId { get; set; } = 0;
        public string ConsecutivoPedidoCreado { get; set; } = null!;
    }
}
