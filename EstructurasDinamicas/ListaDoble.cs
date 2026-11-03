namespace EstructurasDinamicas;

/// <summary>Nodo de la lista doblemente enlazada: dato y referencias al anterior y al siguiente.</summary>
public class NodoDoble<T>
{
    public T Dato;
    public NodoDoble<T>? Anterior;
    public NodoDoble<T>? Siguiente;

    public NodoDoble(T dato) => Dato = dato;
}

/// <summary>
/// Lista doblemente enlazada con nodos dinámicos. Conserva referencias a la cabeza y a la cola,
/// por lo que insertar o eliminar en cualquiera de los dos extremos cuesta O(1).
/// </summary>
public class ListaDoble<T>
{
    private NodoDoble<T>? cabeza;
    private NodoDoble<T>? cola;

    public int Cantidad { get; private set; }
    public bool EstaVacia => Cantidad == 0;

    public void InsertarAlInicio(T dato)
    {
        var nuevo = new NodoDoble<T>(dato) { Siguiente = cabeza };
        if (cabeza is null) cola = nuevo; else cabeza.Anterior = nuevo;
        cabeza = nuevo;
        Cantidad++;
    }

    public void InsertarAlFinal(T dato)
    {
        var nuevo = new NodoDoble<T>(dato) { Anterior = cola };
        if (cola is null) cabeza = nuevo; else cola.Siguiente = nuevo;
        cola = nuevo;
        Cantidad++;
    }

    /// <summary>Inserta en la posición indicada (0 = inicio, Cantidad = final).</summary>
    public void InsertarEn(int posicion, T dato)
    {
        if (posicion < 0 || posicion > Cantidad) throw new ArgumentOutOfRangeException(nameof(posicion));
        if (posicion == 0) { InsertarAlInicio(dato); return; }
        if (posicion == Cantidad) { InsertarAlFinal(dato); return; }

        NodoDoble<T> siguiente = NodoEn(posicion);
        var nuevo = new NodoDoble<T>(dato) { Anterior = siguiente.Anterior, Siguiente = siguiente };
        siguiente.Anterior!.Siguiente = nuevo;   // se enlazan los cuatro punteros
        siguiente.Anterior = nuevo;
        Cantidad++;
    }

    public T EliminarAlInicio()
    {
        if (cabeza is null) throw new InvalidOperationException("La lista está vacía.");
        T dato = cabeza.Dato;
        Desenlazar(cabeza);
        return dato;
    }

    public T EliminarAlFinal()
    {
        if (cola is null) throw new InvalidOperationException("La lista está vacía.");
        T dato = cola.Dato;
        Desenlazar(cola);
        return dato;
    }

    /// <summary>Elimina la primera aparición del valor. Devuelve false si no existe.</summary>
    public bool Eliminar(T dato)
    {
        for (var actual = cabeza; actual is not null; actual = actual.Siguiente)
        {
            if (EqualityComparer<T>.Default.Equals(actual.Dato, dato))
            {
                Desenlazar(actual);
                return true;
            }
        }
        return false;
    }

    /// <summary>Devuelve la posición del valor o -1 si no está.</summary>
    public int Buscar(T dato)
    {
        int i = 0;
        for (var actual = cabeza; actual is not null; actual = actual.Siguiente, i++)
            if (EqualityComparer<T>.Default.Equals(actual.Dato, dato)) return i;
        return -1;
    }

    public T ObtenerEn(int posicion) => NodoEn(posicion).Dato;

    public IEnumerable<T> RecorrerAdelante()
    {
        for (var actual = cabeza; actual is not null; actual = actual.Siguiente) yield return actual.Dato;
    }

    public IEnumerable<T> RecorrerAtras()
    {
        for (var actual = cola; actual is not null; actual = actual.Anterior) yield return actual.Dato;
    }

    /// <summary>Llega al nodo por el extremo más cercano: como máximo recorre la mitad de la lista.</summary>
    private NodoDoble<T> NodoEn(int posicion)
    {
        if (posicion < 0 || posicion >= Cantidad) throw new ArgumentOutOfRangeException(nameof(posicion));
        if (posicion < Cantidad / 2)
        {
            var actual = cabeza!;
            for (int i = 0; i < posicion; i++) actual = actual.Siguiente!;
            return actual;
        }
        var desdeCola = cola!;
        for (int i = Cantidad - 1; i > posicion; i--) desdeCola = desdeCola.Anterior!;
        return desdeCola;
    }

    /// <summary>Saca un nodo de la cadena reconectando a sus vecinos.</summary>
    private void Desenlazar(NodoDoble<T> nodo)
    {
        if (nodo.Anterior is null) cabeza = nodo.Siguiente; else nodo.Anterior.Siguiente = nodo.Siguiente;
        if (nodo.Siguiente is null) cola = nodo.Anterior; else nodo.Siguiente.Anterior = nodo.Anterior;
        nodo.Anterior = nodo.Siguiente = null;
        Cantidad--;
    }
}