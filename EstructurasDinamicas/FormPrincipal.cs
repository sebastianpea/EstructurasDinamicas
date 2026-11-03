using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EstructurasDinamicas
{
    public partial class FormPrincipal : Form
    {
        private readonly ListaDoble<int> lista = new();

        private readonly NumericUpDown nudValor = new() { Minimum = -9999, Maximum = 9999, Value = 10, Width = 80 };
        private readonly NumericUpDown nudPosicion = new() { Minimum = 0, Maximum = 0, Width = 60 };
        private readonly FlowLayoutPanel panelNodos = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke };
        private readonly TextBox txtBitacora = CrearTexto();
        private readonly TextBox txtColecciones = CrearTexto();
        private readonly NumericUpDown nudN = new() { Minimum = 10_000, Maximum = 2_000_000, Increment = 100_000, Value = 1_000_000, Width = 110, ThousandsSeparator = true };
        private readonly Button btnComparar = new() { Text = "Ejecutar comparativa", AutoSize = true };
        private readonly DataGridView dgv = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window
        };

        public FormPrincipal()
        {
            Text = "Tarea 3.2 - Estructuras dinámicas y comparativa";
            Size = new Size(1100, 680);
            MinimumSize = new Size(850, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            var pestanas = new TabControl { Dock = DockStyle.Fill };
            pestanas.TabPages.Add(CrearPestanaLista());
            pestanas.TabPages.Add(CrearPestanaColecciones());
            pestanas.TabPages.Add(CrearPestanaComparativa());
            Controls.Add(pestanas);
            Dibujar();
        }

        // ---------------------------------------------------------------- Lista doble
        private TabPage CrearPestanaLista()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            barra.Controls.AddRange(new Control[] { Etiqueta("Valor:"), nudValor, Etiqueta("Posición:"), nudPosicion });
            Boton(barra, "Insertar al inicio", () => { lista.InsertarAlInicio(V); return $"InsertarAlInicio({V})"; });
            Boton(barra, "Insertar al final", () => { lista.InsertarAlFinal(V); return $"InsertarAlFinal({V})"; });
            Boton(barra, "Insertar en posición", () => { lista.InsertarEn((int)nudPosicion.Value, V); return $"InsertarEn({nudPosicion.Value}, {V})"; });
            Boton(barra, "Eliminar inicio", () => $"EliminarAlInicio() = {lista.EliminarAlInicio()}");
            Boton(barra, "Eliminar final", () => $"EliminarAlFinal() = {lista.EliminarAlFinal()}");
            Boton(barra, "Eliminar valor", () => $"Eliminar({V}) = {lista.Eliminar(V)}");
            Boton(barra, "Buscar", () => $"Buscar({V}) = posición {lista.Buscar(V)}");
            Boton(barra, "Recorrer hacia atrás", () => "Recorrido desde la cola: " + string.Join(" <- ", lista.RecorrerAtras()));

            var tabla = Vertical(barra, panelNodos, txtBitacora, 150);
            return Pagina("1. Lista doblemente enlazada", tabla);
        }

        private int V => (int)nudValor.Value;

        private void Boton(FlowLayoutPanel barra, string texto, Func<string> accion)
        {
            var b = new Button { Text = texto, AutoSize = true };
            b.Click += (s, e) =>
            {
                try { txtBitacora.AppendText(accion() + Environment.NewLine); }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
                { txtBitacora.AppendText("ERROR: " + ex.Message + Environment.NewLine); }
                Dibujar();
            };
            barra.Controls.Add(b);
        }

        /// <summary>Dibuja: null ← [10] ⇄ [20] ⇄ [30] → null, con cabeza y cola señaladas.</summary>
        private void Dibujar()
        {
            panelNodos.SuspendLayout();
            panelNodos.Controls.Clear();
            panelNodos.Controls.Add(Caja("null", Color.Gainsboro, 50));
            int i = 0;
            foreach (int dato in lista.RecorrerAdelante())
            {
                panelNodos.Controls.Add(Flecha(i == 0 ? "←" : "⇄"));
                string marca = i == 0 && i == lista.Cantidad - 1 ? "cabeza/cola" : i == 0 ? "cabeza" : i == lista.Cantidad - 1 ? "cola" : $"[{i}]";
                panelNodos.Controls.Add(Caja($"{marca}{Environment.NewLine}{dato}", Color.LightSkyBlue, 80));
                i++;
            }
            panelNodos.Controls.Add(Flecha(lista.Cantidad == 0 ? "" : "→"));
            panelNodos.Controls.Add(Caja("null", Color.Gainsboro, 50));
            panelNodos.ResumeLayout();
            nudPosicion.Maximum = lista.Cantidad;
        }

        private static Label Caja(string t, Color c, int ancho) => new()
        {
            Text = t,
            Size = new Size(ancho, 52),
            BackColor = c,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(2, 8, 2, 8)
        };

        private static Label Flecha(string t) => new()
        {
            Text = t,
            Size = new Size(30, 52),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 8)
        };

        // ---------------------------------------------------------------- Colecciones genéricas
        private TabPage CrearPestanaColecciones()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            var b = new Button { Text = "Ejecutar demostración de Stack<T>, Queue<T>, LinkedList<T> y List<T>", AutoSize = true };
            b.Click += (s, e) => txtColecciones.AppendText(Comparativa.DemostrarColeccionesGenericas() + Environment.NewLine);
            barra.Controls.Add(b);

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(txtColecciones, 0, 1);
            return Pagina("2. Colecciones genéricas de .NET", tabla);
        }

        // ---------------------------------------------------------------- Comparativa
        private TabPage CrearPestanaComparativa()
        {
            foreach (string c in new[] { "Estructura", "Tipo", "Memoria (MB)", "Bytes por elemento",
                                     "Insertar n al final (ms)", $"Insertar {Comparativa.ElementosAlInicio:N0} al inicio (ms)", "Recorrer (ms)" })
                dgv.Columns.Add(c, c);
            dgv.Columns[0].FillWeight = 180;
            dgv.Columns[1].FillWeight = 160;

            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            barra.Controls.AddRange(new Control[] { Etiqueta("Elementos (n):"), nudN, btnComparar });
            btnComparar.Click += BtnComparar_Click;

            var nota = Etiqueta("El segundo arreglo se reserva con 10 veces la capacidad necesaria para mostrar el desperdicio de la memoria estática.");
            nota.ForeColor = Color.DimGray;

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(nota, 0, 1);
            tabla.Controls.Add(dgv, 0, 2);
            return Pagina("3. Comparativa estática vs. dinámica", tabla);
        }

        private async void BtnComparar_Click(object? sender, EventArgs e)
        {
            int n = (int)nudN.Value;
            btnComparar.Enabled = false;
            UseWaitCursor = true;
            dgv.Rows.Clear();
            try
            {
                var filas = await Task.Run(() => Comparativa.Ejecutar(n, n * 10));
                foreach (var f in filas)
                {
                    dgv.Rows.Add(f.Estructura, f.Tipo, (f.Bytes / 1048576.0).ToString("F2"), f.BytesPorElemento.ToString("F1"),
                        f.MsInsertarFinal.ToString("F2"), double.IsNaN(f.MsInsertarInicio) ? "—" : f.MsInsertarInicio.ToString("F2"),
                        f.MsRecorrer.ToString("F2"));
                }
            }
            finally
            {
                btnComparar.Enabled = true;
                UseWaitCursor = false;
            }
        }

        // ---------------------------------------------------------------- Auxiliares
        private static TableLayoutPanel Vertical(Control arriba, Control centro, Control abajo, int altoAbajo)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, altoAbajo));
            t.Controls.Add(arriba, 0, 0);
            t.Controls.Add(centro, 0, 1);
            t.Controls.Add(abajo, 0, 2);
            return t;
        }

        private static TabPage Pagina(string titulo, Control contenido)
        {
            var p = new TabPage(titulo) { Padding = new Padding(6) };
            p.Controls.Add(contenido);
            return p;
        }

        private static TextBox CrearTexto() => new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 10F),
            BackColor = Color.White
        };

        private static Label Etiqueta(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(3, 9, 3, 3) };
    }
}
