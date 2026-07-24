using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Telemetri_tasarım_denemesi
{
    public static class AppState
    {
        static byte[] buffer = new byte[13];
        public static SerialPort SerialPort { get; set; }
        public static Thread okumaThread { get; set; }
        public static byte hiz { get; set; }
        public static byte voltaj { get; set; }
        public static byte sicaklik { get; set; }
        public static byte enerji { get; set; }
        public static string SecilenPort { get; set; }
        public static string SecilenRate { get; set; }

        private static readonly object kayitlock = new object();

        public static System.Threading.Timer yazimTimer;

        public static System.Threading.Timer PortTimer;

        public static bool RecordFlag;

        public static string dosyaYolu;

        public static string KayıtDosya;

        public static string ExKayıtDosya;

        public static bool BaslıkYazıldi = false;

        public static int KayıtSayaci = 0;

        public static int GelenByteIndex = 0;

        public static int KayipPaket;

        public static uint Car_Ms_Since_Started;

        public static byte OncekiSeq;

        public static string TopluYazi;

        public static string YeniSatırlar;

        public static ConcurrentQueue<string> yazimKuyrugu = new ConcurrentQueue<string>();

        public static DateTime SonVeriZamani = DateTime.MinValue;

        public static bool IlkVeriGeldi = false;

        public static volatile bool calisiyor;

        public static List<string> BekleyenSatırlar = new List<string>();

        public static string KayitYap()
        {
            lock (kayitlock)
            {
                try
                {
                    if (RecordFlag == false)
                    {
                        return "";
                    }
                    if (BaslıkYazıldi == false)
                    {
                        KayıtDosya = dosyaYolu;
                        ExKayıtDosya = dosyaYolu;
                        BaslıkYazıldi = true;
                        yazimKuyrugu.Enqueue("zaman_ms;  hiz_kmh; T_bat_C; V_bat_C; kalan_enerji_Wh");
                    }
                    YeniSatırlar =
                    $"{Car_Ms_Since_Started};" +
                    $"{AppState.hiz};" +
                    $"{AppState.sicaklik};" +
                    $"{AppState.voltaj};" +
                    $"{AppState.enerji}";
                    yazimKuyrugu.Enqueue(YeniSatırlar);
                    KayıtSayaci++;
                    return YeniSatırlar;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[KayitYap HATA] {ex.Message}");
                    return "";
                }
            }
        }

        public enum BaglantiDurumu
        {
            Bagli,
            Kopuk,
            YenidenBaglaniyor,
        }

        public static void ByteIsle(byte gelen)
        {
            System.Diagnostics.Debug.WriteLine($"[UART] gelen=0x{gelen:X2}");

            if (GelenByteIndex == 0)
            {
                if (gelen == 0xFF)
                {
                    buffer[0] = gelen;
                    GelenByteIndex = 1;
                }
            }
            else if (GelenByteIndex == 1)
            {
                if (gelen == 4)
                {
                    buffer[1] = gelen;
                    GelenByteIndex = 2;
                }
                else
                {
                    GelenByteIndex = 0;
                }
            }
            else if (GelenByteIndex == 2)
            {
                buffer[2] = gelen;
                GelenByteIndex = 3;
            }
            else if (GelenByteIndex == 3)
            {
                buffer[3] = gelen;
                GelenByteIndex = 4;
            }
            else if (GelenByteIndex == 4)
            {
                buffer[4] = gelen;
                GelenByteIndex = 5;
            }
            else if (GelenByteIndex == 5)
            {
                buffer[5] = gelen;
                GelenByteIndex = 6;
            }
            else if (GelenByteIndex == 6)
            {
                buffer[6] = gelen;
                GelenByteIndex = 7;
            }
            else if (GelenByteIndex == 7)
            {
                buffer[7] = gelen;
                GelenByteIndex = 8;
            }
            else if (GelenByteIndex == 8)
            {
                buffer[8] = gelen;
                GelenByteIndex = 9;
            }
            else if (GelenByteIndex == 9)
            {
                buffer[9] = gelen;
                GelenByteIndex = 10;
            }
            else if (GelenByteIndex == 10)
            {
                buffer[10] = gelen;
                GelenByteIndex = 11;
            }
            else if (GelenByteIndex == 11)
            {
                buffer[11] = gelen;
                GelenByteIndex = 12;
            }
            else if (GelenByteIndex == 12)
            {
                int CRC = (byte)(buffer[2] + buffer[3] + buffer[4] + buffer[5] + buffer[6] + buffer[7] + buffer[8] + buffer[9] + buffer[10] + buffer[11]) & 0xFF;
                if (gelen == CRC)
                {
                    if (AppState.IlkVeriGeldi == false)
                    {
                        IlkVeriGeldi = true;
                        OncekiSeq = buffer[6];
                    }
                    else
                    {
                        byte fark = (byte)(buffer[6] - OncekiSeq);
                        if (fark > 1)
                        {
                            KayipPaket += (fark - 1);
                        }
                    }
                    voltaj = buffer[2];
                    hiz = buffer[3];
                    sicaklik = buffer[4];
                    enerji = buffer[5];
                    Car_Ms_Since_Started = BitConverter.ToUInt32(buffer, 8);
                    GelenByteIndex = 0;
                    KayitYap();
                    SonVeriZamani = DateTime.Now;
                    if (buffer[7] == 1)
                    {
                        byte[] ACK_Byte = new byte[] { 1 };
                        SerialPort.Write(ACK_Byte, 0, 1);
                    }
                    OncekiSeq = buffer[6];
                }
                else
                {
                    GelenByteIndex = 0;
                }
            }
        }

        private static void DiskeYaz(object state)
        {
            try
            {
                if (yazimKuyrugu.IsEmpty) return;
                if (string.IsNullOrEmpty(KayıtDosya)) return;

                StringBuilder sb = new StringBuilder();
                while (yazimKuyrugu.TryDequeue(out var satir))
                {
                    sb.AppendLine(satir);
                }

                File.AppendAllText(KayıtDosya, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DiskeYaz HATA] {ex.Message}");
            }
        }
        public static void okumaDongusu()
        {
            while (calisiyor == true)
            {
                try
                {
                    if (SerialPort.BytesToRead > 0)
                    {
                        byte gelen = (byte)SerialPort.ReadByte();
                        ByteIsle(gelen);
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UART HATA] {ex}");
                    calisiyor = false;
                    Durum = BaglantiDurumu.Kopuk;
                    try
                    {
                        SerialPort.Close();
                    }
                    catch (Exception CloseEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Close Hatası] {CloseEx}");
                    }
                }
            }
        }

        public static void StartListening()
        {
            if (SerialPort != null)
            {
                Durum = BaglantiDurumu.Bagli;
                calisiyor = true;
                okumaThread = new Thread(okumaDongusu);
                okumaThread.IsBackground = true;
                okumaThread.Start();
                yazimTimer = new System.Threading.Timer(DiskeYaz, null, 200, 200);
            }
        }
        public static void StopListening()
        {
            if (SerialPort != null)
            {
                calisiyor = false;
                okumaThread?.Join(500);
                yazimTimer?.Dispose();
            }
        }

        public static void BaglantiIzlemeyiBaslat()
        {
            PortTimer = new System.Threading.Timer(YenidenBaglanmayiDene, null, 2000, 2000);
        }

        public static void YenidenBaglanmayiDene(object state)
        {
            if (Durum != BaglantiDurumu.Kopuk) return;
            if (string.IsNullOrEmpty(SecilenPort)) return;
            if (SerialPort.GetPortNames().Contains(SecilenPort))
            {
                Durum = BaglantiDurumu.YenidenBaglaniyor;
                try
                {
                    try { SerialPort?.Dispose(); } catch { }
                    int rate = int.Parse(SecilenRate);
                    SerialPort = new SerialPort(SecilenPort, rate);
                    SerialPort.Open();
                    StartListening();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Yeniden Bağlanma Hatası] {ex.Message}");
                    Durum = BaglantiDurumu.Kopuk;
                }
            }
        }

        public static event Action<BaglantiDurumu> DurumDegisti;
        private static BaglantiDurumu _Durum = BaglantiDurumu.Kopuk;
        public static BaglantiDurumu Durum
        {
            get => _Durum;
            set
            {
                _Durum = value;
                DurumDegisti?.Invoke(value);
            }
        }
    }
}

