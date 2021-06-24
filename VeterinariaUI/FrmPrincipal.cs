using System;
using System.Threading;
using System.Windows.Forms;
using Entidades;


namespace VeterinariaUI
{
    public partial class FrmPrincipal : Form
    {
        Thread doctorAtendiendo;

        public FrmPrincipal()
        {
            InitializeComponent();
            doctorAtendiendo = new Thread(Veterinaria.Comenzar);
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            doctorAtendiendo.Name = "DOCTOR ATENDIENDO";
            doctorAtendiendo.Start();
        }

    }
}
