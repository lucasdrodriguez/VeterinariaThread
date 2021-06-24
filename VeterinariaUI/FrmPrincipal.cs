using System;
using System.Threading;
using System.Windows.Forms;
using Entidades;


namespace VeterinariaUI
{
    public partial class FrmPrincipal : Form
    {
        Thread t1;

        public FrmPrincipal()
        {
            InitializeComponent();
            t1 = new Thread(Veterinaria.Comenzar);
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            t1.Start();
        }




    }
}
