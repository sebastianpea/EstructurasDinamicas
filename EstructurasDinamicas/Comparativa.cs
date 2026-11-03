using System.Diagnostics;
using System.Text;

namespace EstructurasDinamicas;

/// <summary>Resultado de medir una estructura con n elementos.</summary>
public record FilaComparativa(string Estructura, string Tipo, long Bytes, double BytesPorElemento,
                              double MsInsertarFinal, double MsInsertarInicio, double MsRecorrer);

/// <summary>Mide memoria y tiempo de estructuras estáticas y dinámicas con la misma cantidad de datos.</summary>
public static class Comparativa
{
    /// <summary>Cantidad de inserciones al inicio que se miden (List desplaza todo en cada una).</summary>
    public const int ElementosAlInicio = 20_000;

    /// <summary>Demostración de las colecciones genéricas predefinidas de .NET.</summary>
    public static string DemostrarColeccionesGenericas()
    {
        var t = new StringBuilder();

        var pila = new Stack<string>();
        pila.Push("A"); pila.Push("B"); pila.Push("C");
        t.AppendLine($"Stack<string>: Push A, B, C -> Peek = {pila.Peek()}, Pop = {pila.Pop()}, quedan {pila.Count}: [{string.Join(", ", pila)}]");

        var cola = new Queue<string>();
        cola.Enqueue("A"); cola.Enqueue("B"); cola.Enqueue("C");
        t.AppendLine($"Queue<string>: Enqueue A, B, C -> Peek = {cola.Peek()}, Dequeue = {cola.Dequeue()}, quedan {cola.Count}: [{string.Join(", ", cola)}]");

        var enlazada = new LinkedList<string>();
        enlazada.AddLast("B"); enlazada.AddFirst("A"); enlazada.AddLast("C");
        enlazada.AddAfter(enlazada.Find("B")!, "B2");
        t.AppendLine($"LinkedList<string>: AddLast B, AddFirst A, AddLast C, AddAfter(B, B2) -> [{string.Join(", ", enlazada)}]");

        var lista = new List<string>();
        t.Append("List<string>: capacidad al agregar 1..9 elementos -> ");
        for (int i = 1; i <= 9; i++) { lista.Add("x" + i); t.Append(lista.Capacity + (i < 9 ? ", " : "")); }
        t.AppendLine();
        return t.ToString();
    }

    public static List<FilaComparativa> Ejecutar(int n, int capacidadEstatica)
    {
        Ejecutar0(1000, 1000);   // calentamiento (compilación JIT)
        return Ejecutar0(n, capacidadEstatica);
    }

    private static List<FilaComparativa> Ejecutar0(int n, int capacidadEstatica)
    {
        var filas = new List<FilaComparativa>
        {
            Medir("Arreglo int[] (capacidad = n)", "Estática", n, () => new int[n],
                  (o, i) => ((int[])o)[i] = i, null, o => Sumar((int[])o)),
            Medir($"Arreglo int[] (capacidad {capacidadEstatica:N0})", "Estática", n, () => new int[capacidadEstatica],
                  (o, i) => ((int[])o)[i] = i, null, o => Sumar((int[])o)),
            Medir("List<int>", "Dinámica (arreglo redimensionable)", n, () => new List<int>(),
                  (o, i) => ((List<int>)o).Add(i), (o, i) => ((List<int>)o).Insert(0, i), o => { long s = 0; foreach (int v in (List<int>)o) s += v; return s; }),
            Medir("ListaDoble<int> (propia)", "Dinámica (nodos)", n, () => new ListaDoble<int>(),
                  (o, i) => ((ListaDoble<int>)o).InsertarAlFinal(i), (o, i) => ((ListaDoble<int>)o).InsertarAlInicio(i), o => { long s = 0; foreach (int v in ((ListaDoble<int>)o).RecorrerAdelante()) s += v; return s; }),
            Medir("LinkedList<int>", "Dinámica (nodos)", n, () => new LinkedList<int>(),
                  (o, i) => ((LinkedList<int>)o).AddLast(i), (o, i) => ((LinkedList<int>)o).AddFirst(i), o => { long s = 0; foreach (int v in (LinkedList<int>)o) s += v; return s; }),
            Medir("Stack<int>", "Dinámica (arreglo redimensionable)", n, () => new Stack<int>(),
                  (o, i) => ((Stack<int>)o).Push(i), null, o => { long s = 0; foreach (int v in (Stack<int>)o) s += v; return s; }),
            Medir("Queue<int>", "Dinámica (arreglo circular)", n, () => new Queue<int>(),
                  (o, i) => ((Queue<int>)o).Enqueue(i), null, o => { long s = 0; foreach (int v in (Queue<int>)o) s += v; return s; }),
        };
        return filas;
    }

    private static long Sumar(int[] a) { long s = 0; foreach (int v in a) s += v; return s; }

    /// <summary>
    /// Mide la memoria ocupada tras insertar n elementos al final, el tiempo de esas inserciones,
    /// el de insertar 20 000 elementos al inicio (si aplica) y el de recorrer.
    /// </summary>
    private static FilaComparativa Medir(string nombre, string tipo, int n, Func<object> crear,
        Action<object, int> insertarFinal, Action<object, int>? insertarInicio, Func<object, long> recorrer)
    {
        long antes = GC.GetTotalMemory(true);
        var cronometro = Stopwatch.StartNew();
        object estructura = crear();
        for (int i = 0; i < n; i++) insertarFinal(estructura, i);
        double msFinal = cronometro.Elapsed.TotalMilliseconds;
        long bytes = GC.GetTotalMemory(true) - antes;

        cronometro.Restart();
        recorrer(estructura);
        double msRecorrer = cronometro.Elapsed.TotalMilliseconds;
        GC.KeepAlive(estructura);

        double msInicio = double.NaN;
        if (insertarInicio is not null)
        {
            int m = Math.Min(n, ElementosAlInicio);
            object otra = crear();
            cronometro.Restart();
            for (int i = 0; i < m; i++) insertarInicio(otra, i);
            msInicio = cronometro.Elapsed.TotalMilliseconds;
        }

        return new FilaComparativa(nombre, tipo, bytes, (double)bytes / n, msFinal, msInicio, msRecorrer);
    }
}