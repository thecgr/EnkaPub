using EnkaPub.DATA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnkaPub.UI
{
    public partial class SiparisForm : Form
    {
        public event EventHandler<MasaTasindiEventArgs> MasaTasindi;

        private readonly KafeVeri _db;
        private readonly Siparis _siparis;
        private readonly BindingList<SiparisDetay> _blSiparisDetaylar;

        public SiparisForm(KafeVeri db, Siparis siparis)
        {
            _db = db;
            _siparis = siparis;
            _blSiparisDetaylar = new BindingList<SiparisDetay>(_siparis.SiparisDetaylar);
            InitializeComponent();
            cboUrun.DataSource = _db.Urunler;
            dgvSiparisDetaylar.DataSource = _blSiparisDetaylar;
            _blSiparisDetaylar.ListChanged += _blSiparisDetaylar_ListChanged;
            MasaNoGuncelle();
            OdemeTutariniGuncelle();
        }

        private void _blSiparisDetaylar_ListChanged(object sender, ListChangedEventArgs e)
        {
            OdemeTutariniGuncelle();
        }

        private void OdemeTutariniGuncelle()
        {
            lblOdemeTutari.Text = _siparis.ToplamTutarTL;
        }

        private void MasaNoGuncelle()
        {
            Text = $"Masa {_siparis.MasaNo:00} (Açılış zamanı: {_siparis.AcilisZamani})";
            lblMasaNo.Text = _siparis.MasaNo.ToString("00");

            cboMasaNo.DataSource = Enumerable
                .Range(1, _db.MasaAdet)
                .Where(x => !_db.AktifSiparisler.Any(s => s.MasaNo ==x))
                .ToList();
            //cboMasaNo.Items.Clear();
            //for (int i = 1; i <= _db.MasaAdet; i++)
            //{
            //    if (!_db.AktifSiparisler.Any(x => x.MasaNo == i)) 
            //    cboMasaNo.Items.Add(i);
            //}

        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            if (cboUrun.SelectedIndex == -1) return;
            Urun urun = (Urun)cboUrun.SelectedItem;

            SiparisDetay sd = new SiparisDetay()
            {
                UrunAd = urun.UrunAd,
                BirimFiyat = urun.BirimFiyat,
                Adet = (int)nudAdet.Value
            };
            _blSiparisDetaylar.Add(sd);
            EkleFormunuSifirla();

        }

        private void EkleFormunuSifirla()
        {
            cboUrun.SelectedIndex = 0;
            nudAdet.Value = 1;
        }

        private void dgvSiparisDetaylar_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                text: "Silmek istediğinize emin misiniz?",
                caption: "Detay Silme Onayı",
                buttons: MessageBoxButtons.YesNo,
                icon: MessageBoxIcon.Question,
                defaultButton: MessageBoxDefaultButton.Button2);

            e.Cancel = dr == DialogResult.No;
        }

        private void btnAnasayfa_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOdemeAl_Click(object sender, EventArgs e)
        {
            MasayiKapat(SiparisDurum.Odendi, _siparis.ToplamTutar());
        }

        private void btnSiparisIptal_Click(object sender, EventArgs e)
        {
            MasayiKapat(SiparisDurum.Iptal);
        }
        private void MasayiKapat(SiparisDurum durum, decimal odeneTutar = 0)
        {
            _siparis.Durum = durum;
            _siparis.KapanisZamani = DateTime.Now;
            _siparis.OdenenTutar = odeneTutar;
            _db.AktifSiparisler.Remove(_siparis);
            _db.GecmisSiparisler.Add(_siparis);
            Close();
        }


        private void btnTasi_Click(object sender, EventArgs e)
        {
            if (cboMasaNo.SelectedItem == null) return;
            int eski = _siparis.MasaNo;
            int yeni = (int)cboMasaNo.SelectedItem;
            _siparis.MasaNo = yeni;
            MasaNoGuncelle();
            if (MasaTasindi != null)
                MasaTasindi(this, new MasaTasindiEventArgs(eski, yeni));
        }
    }
}
