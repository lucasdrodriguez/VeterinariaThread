using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Perro
    {
        string nombre;
        int edad;

        public Perro()
        {

        }
        public Perro(string nomb, int edad)
        {
            this.nombre = nomb;
            this.edad = edad;
        }
        public string Nombre { get => nombre; set => nombre = value; }
        public int Edad { get => edad; set => edad = value; }


        public override string ToString()
        {
            return this.nombre;
        }

        public string DatosCompletos()
        {

            return this.nombre + "  " + this.edad;
        }

    }
}
   