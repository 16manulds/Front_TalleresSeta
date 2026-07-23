namespace Front_TalleresSeta.Compartido.Auxiliares
{
    public abstract class EnumeracionesAux : IComparable
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; }

        protected EnumeracionesAux() { }

        protected EnumeracionesAux(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public override string ToString() => Nombre;

        public int CompareTo(object obj)
        {
            return Id.CompareTo(((EnumeracionesAux)obj).Id);
        }
    }
}
