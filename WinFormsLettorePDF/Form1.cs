

using System.Windows.Forms.Design.Behavior;
using static UglyToad.PdfPig.Core.PdfSubpath;

namespace WinFormsLettorePDF
{

    public partial class Form1 : Form
    {
        private bool play = false;
        private string filePath = "";
        private int page = 0;
        private int pageIndex = 0;
        private List<List<string>> pages = [];
        private const int WM_APPCOMMAND = 0x0319;
        private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        private const int APPCOMMAND_MEDIA_STOP = 13;
        private const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
        private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_APPCOMMAND)
            {
                int cmd = GET_APPCOMMAND_LPARAM(m.LParam);

                switch (cmd)
                {
                    case APPCOMMAND_MEDIA_PLAY_PAUSE:
                        button1_Click(null!, null!);
                        m.Result = IntPtr.Zero;
                        return;

                    case APPCOMMAND_MEDIA_STOP:
                        m.Result = IntPtr.Zero;
                        return;

                    case APPCOMMAND_MEDIA_NEXTTRACK:
                        m.Result = IntPtr.Zero;
                        return;

                    case APPCOMMAND_MEDIA_PREVIOUSTRACK:
                        m.Result = IntPtr.Zero;
                        return;
                }
            }

            base.WndProc(ref m);
        }

        private static int GET_APPCOMMAND_LPARAM(IntPtr lParam)
        {
            return ((short)(((long)lParam >> 16) & 0xFFFF)) & ~0xF000;
        }

        public Form1()
        {
            InitializeComponent();
            var (path, indice, indicePagina) = Funzioni.Load();
            filePath = path;
            page = indice;
            pageIndex = indicePagina;
            textBox1.Text = filePath;
            numericUpDown1.Value = page;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!play)
            {
                play = true;
                button1.Text = "Stop";
                try
                {
                    this.pages = Funzioni.Extract(this.filePath);
                }
                catch
                {
                    play = false;
                    button1.Text = "Play";
                    MessageBox.Show("Errore nell'apertura del file PDF.");
                    return;
                }
                if (page >= pages.Count())
                {
                    play = false;
                    button1.Text = "Play";
                    MessageBox.Show($"Pagina oltre il limite del documento ({pages.Count}).");
                }
                var pagina = pages[this.page];
                Task.Run(() =>
                {
                    while (page < pages.Count)
                    {
                        LeggiPagina(pagina);
                        if (!play) break;
                        numericUpDown1.Value = ++page;
                        pagina = pages[page];
                    }
                });
            }
            else
            {
                play = false;
                button1.Text = "Play";
                Funzioni.StopLine();
            }
        }

        private void LeggiPagina(List<string> pagina)
        {
            for (pageIndex = pageIndex >= pagina.Count ? 0 : pageIndex; pageIndex < pagina.Count; pageIndex++)
            {
                Funzioni.ReadLine(pagina[pageIndex]);
                Funzioni.Save(this.filePath, this.page, this.pageIndex);
                if (!play) return;
            }
            pageIndex = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (play) textBox1.Text = this.filePath;
            else this.filePath = textBox1.Text;
            Funzioni.Save(this.filePath, this.page, this.pageIndex);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (play) numericUpDown1.Value = this.page;
            else this.page = (int)numericUpDown1.Value;
            Funzioni.Save(this.filePath, this.page, this.pageIndex);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
