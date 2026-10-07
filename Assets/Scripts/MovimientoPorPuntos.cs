using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mueve un objeto 2D (por ejemplo un círculo) por una lista de puntos.
/// Los puntos se meten en una cola; se van sacando de uno en uno y, cuando
/// la cola se queda vacía, se vuelve a llenar con todos los puntos. Infinito.
///
/// NUEVO: cada vez que llega a un punto cambia de color con uno sacado de una pila.
/// Cuando la pila se acaba, se rellena con colores aleatorios.
/// (Todo lo nuevo está dentro de las regiones "NUEVO".)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // NUEVO: necesario para cambiar el color
public class MovimientoPorPuntos : MonoBehaviour
{
    [Header("Recorrido")]
    [Tooltip("Arrastra aquí, en orden, los objetos que marcan los puntos del recorrido.")]
    [SerializeField] private Transform[] puntos;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 3f;
    [Tooltip("Distancia a la que se considera que el círculo ha llegado al punto.")]
    [SerializeField] private float distanciaLlegada = 0.05f;

    #region NUEVO: campos de colores
    [Header("NUEVO - Colores")]
    [Tooltip("Colores iniciales de la pila. El primero de la lista es el primero que se usa.")]
    [SerializeField] private Color[] coloresIniciales;
    [Tooltip("Cuántos colores aleatorios se generan cada vez que la pila se queda vacía.")]
    [SerializeField] private int coloresAleatorios = 5;

    private readonly Stack<Color> pilaColores = new Stack<Color>();
    private SpriteRenderer spriteRenderer;
    #endregion

    private readonly Queue<Transform> cola = new Queue<Transform>();
    private Transform destinoActual;

    private void Start()
    {
        if (puntos == null || puntos.Length == 0)
        {
            Debug.LogWarning($"{name}: no hay puntos asignados en el inspector.", this);
            enabled = false;
            return;
        }

        #region NUEVO: preparar la pila de colores
        spriteRenderer = GetComponent<SpriteRenderer>();
        CargarColoresIniciales();
        #endregion

        // Empieza colocado en el primer punto de la cola.
        SiguienteDestino();
        transform.position = PosicionPunto(destinoActual);
    }

    private void Update()
    {
        Vector3 destino = PosicionPunto(destinoActual);

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime);

        if (Vector2.Distance(transform.position, destino) <= distanciaLlegada)
        {
            CambiarColor(); // NUEVO
            SiguienteDestino();
        }
    }

    // Saca el siguiente punto de la cola. Si está vacía, la rellena de nuevo.
    private void SiguienteDestino()
    {
        if (cola.Count == 0)
        {
            RellenarCola();
        }

        destinoActual = cola.Dequeue();
    }

    private void RellenarCola()
    {
        foreach (Transform punto in puntos)
        {
            if (punto != null)
            {
                cola.Enqueue(punto);
            }
        }
    }

    // Mantiene la Z del objeto para que no cambie la profundidad en 2D.
    private Vector3 PosicionPunto(Transform punto)
    {
        Vector3 p = punto.position;
        p.z = transform.position.z;
        return p;
    }

    #region NUEVO: métodos de colores
    // Saca un color de la pila y se lo aplica al círculo.
    // Si la pila está vacía, antes la rellena con colores aleatorios.
    private void CambiarColor()
    {
        if (pilaColores.Count == 0)
        {
            RellenarPilaAleatoria();
        }

        spriteRenderer.color = pilaColores.Pop();
    }

    // Una pila es LIFO: se apilan al revés para que el primero
    // de la lista del inspector sea el primero en salir.
    private void CargarColoresIniciales()
    {
        if (coloresIniciales == null) return;

        for (int i = coloresIniciales.Length - 1; i >= 0; i--)
        {
            pilaColores.Push(coloresIniciales[i]);
        }
    }

    private void RellenarPilaAleatoria()
    {
        int cantidad = Mathf.Max(1, coloresAleatorios);

        for (int i = 0; i < cantidad; i++)
        {
            // Tono aleatorio con saturación y brillo altos para que sean colores vivos.
            pilaColores.Push(Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f));
        }
    }
    #endregion
}