using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Threading;

namespace Entidades
{

    public delegate void ManejarInformacion();

    public static class Veterinaria
    {
        static Thread atenderPerros;
        static ConcurrentQueue<Perro> perrosEnEspera;
        static Stack<Perro> perrosDadosDeAlta;
        static event ManejarInformacion actualizarInfo;
        static readonly string path;
        static readonly string pathLectura;
        static readonly string pathEscritura;

        public static string ProximoTurno
        {
            get
            {
                Perro auxPerro;
                if(perrosEnEspera.TryPeek(out auxPerro))
                {
                    return auxPerro.DatosCompletos();
                }
                return "Sin prox pacientes";
            }

        }

        static Veterinaria()
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            pathLectura = path + @"\Threads\Veterinaria\NuevosPacientes\";
            pathEscritura = path + @"\Threads\Veterinaria\PacientesAtendidos\";

            CrearPaths();

            perrosEnEspera = new ConcurrentQueue<Perro>();
            SetPerrosDefault();


            perrosDadosDeAlta = new Stack<Perro>();


            actualizarInfo += LeerNuevosPacientes;
            actualizarInfo += GenerarBackUpAtendidos;
            actualizarInfo += EliminarAtendidos;

            atenderPerros = new Thread(Atender);
            atenderPerros.Start();
        }

        private static void CrearPaths()
        {
            if (!Directory.Exists(pathLectura))
            {
                Directory.CreateDirectory(pathLectura);
            }
            if (!Directory.Exists(pathEscritura))
            {
                Directory.CreateDirectory(pathEscritura);
            }
        }

        public static void Atender()
        {
            while (true)
            {
                Thread.Sleep(5000);
                Perro perroAtendido;

                if (perrosEnEspera.TryDequeue(out perroAtendido))
                {
                    perrosDadosDeAlta.Push(perroAtendido);
                }
            }
        }


        private static void EliminarAtendidos()
        {

            DirectoryInfo directorioElegido = new DirectoryInfo(pathLectura);
            FileInfo[] files = directorioElegido.GetFiles();

            foreach (FileInfo archivoItem in files)
            {
                if (archivoItem.Name.Contains("Leido_"))
                {
                    File.Delete(archivoItem.FullName);
                }

            }

        }

        private static void SetPerrosDefault()
        {
            perrosEnEspera.Enqueue(new Perro("Pichichus", 5));
            perrosEnEspera.Enqueue(new Perro("Pepe", 10));
            perrosEnEspera.Enqueue(new Perro("Romeo", 11));
            perrosEnEspera.Enqueue(new Perro("Jazmin", 12));
            perrosEnEspera.Enqueue(new Perro("Carola", 5));
        }

        public static void Comenzar()
        {
            while (true)
            {
                Thread.Sleep(5000);
                actualizarInfo.Invoke();
            }
        }

        private static void LeerNuevosPacientes()
        {
            DirectoryInfo directorioElegido = new DirectoryInfo(pathLectura);
            FileInfo[] files = directorioElegido.GetFiles();

            foreach (FileInfo archivoItem in files)
            {
                try
                {

                    using (XmlTextReader xmlReader = new XmlTextReader(archivoItem.FullName))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Perro));
                        perrosEnEspera.Enqueue((Perro)serializer.Deserialize(xmlReader));
                    }

                    File.Move(string.Concat(archivoItem.FullName), string.Concat(pathLectura, "Leido_" + archivoItem.Name));
                }
                catch (Exception e)
                {
                    GuardarLog("LOGS Errores.txt", e.Message.ToString());
                }
            }
        }


        private static void GenerarBackUpAtendidos()
        {
            string[] paths;
            DirectoryInfo directorioElegido = new DirectoryInfo(pathLectura);

            foreach (FileInfo archivoItem in directorioElegido.GetFiles())
            {

                try
                {
                    using (FileStream archivoFileSteam = archivoItem.OpenRead())
                    {
                        if (archivoItem.Extension == ".xml" && archivoItem.FullName.Contains("Leido_"))
                        {
                            using (FileStream ArchivoSteamComprimido = File.Create(archivoItem.FullName + ".zip"))
                            {
                                using (GZipStream objAComprimir = new GZipStream(ArchivoSteamComprimido,
                                   CompressionMode.Compress))
                                {
                                    archivoFileSteam.CopyTo(objAComprimir);
                                }

                                paths = ArchivoSteamComprimido.Name.Split('\\');
                                File.Move(ArchivoSteamComprimido.Name, string.Concat(pathEscritura, paths[paths.Length - 1].ToString()));
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    GuardarLog("LOGSErrores.txt", e.Message.ToString());
                }
            }
        }


        public static void GuardarLog(string archivo, string datos)
        {
            if (!string.IsNullOrEmpty(archivo) && !string.IsNullOrEmpty(datos))
            {
                try
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory;

                    using (StreamWriter sw = new StreamWriter(Path.Combine(path, archivo)))
                    {
                        sw.WriteLine(datos);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }


        public static bool NuevoPacienteSinTurno(Perro nuevoRegistro)
        {
            if (nuevoRegistro != null)
            {
                string pathCompleto = string.Concat(pathLectura, nuevoRegistro.Nombre + ".xml");

                try
                {

                    using (XmlTextWriter xmlWriter = new XmlTextWriter(pathCompleto, Encoding.ASCII))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Perro));
                        serializer.Serialize(xmlWriter, nuevoRegistro);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    GuardarLog("LOGSErrores.txt", e.Message.ToString());

                }
            }
            return false;
        }

    }
}
