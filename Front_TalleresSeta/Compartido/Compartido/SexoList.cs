using Front_TalleresSeta.Compartido.Auxiliares;

namespace Compartida.Compartido
{
    public class SexoList : EnumeracionesAux
    {
        public SexoList(int id, string nombre) : base(id, nombre) { }

        //No se debe modificar o eliminar ningún EstadosList
        //se recomienda agregar uno nuevo
        public static SexoList Hombre = new SexoList(1, "Hombre");
        public static SexoList Mujer = new SexoList(2, "Mujer");
        public static SexoList Otro = new SexoList(3, "Otro");

        public static IList<SexoList> List() => new[] {
            Hombre,
            Mujer,
            Otro
            };

        public static SexoList FiltrarPorNombre(string nombre)
        {
            var result = List().Single(x => String.Equals(x.Nombre, nombre, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        public static SexoList FiltrarporId(int id)
        {
            var result = List().Single(x => x.Id == id);

            return result;
        }

    }
}
