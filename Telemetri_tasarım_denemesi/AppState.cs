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
        static byte[] buffer = new byte[9];
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

        public static bool RecordFlag;

        public static string dosyaYolu;

        public static string KayıtDosya;

        public static string ExKayıtDosya;

        public static bool BaslıkYazıldi = false;

        public static int KayıtSayaci = 0;

        public static int GelenByteIndex = 0;

        public static int KayipPaket;

        public static byte OncekiSeq;

        public static string TopluYazi;

        public static string YeniSatırlar;

        public static ConcurrentQueue<string> yazimKuyrugu = new ConcurrentQueue<string>();

        public static System.Diagnostics.Stopwatch AracStopWatch = new System.Diagnostics.Stopwatch();

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
                        yazimKuyrugu.Enqueue("Zaman_ms;  hiz_kmh; T_bat_C; V_bat_C; kalan_enerji_Wh; Zaman_Clcok;");
                    }
                    YeniSatırlar =
                    $"{AracStopWatch.ElapsedMilliseconds} ms;" +
                    $"{AppState.hiz} km/h;" +
                    $"{AppState.sicaklik} °C;" +
                    $"{AppState.voltaj} V;" +
                    $"{AppState.enerji} wh;" + 
                    $"{DateTime.Now:HH:mm:ss};";
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
                int CRC = (byte)(buffer[2] + buffer[3] + buffer[4] + buffer[5] + buffer[6] + buffer[7]) & 0xFF;
                if (gelen == CRC)
                {
                    if (AppState.IlkVeriGeldi == false)
                    {
                        AracStopWatch.Start();
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
                    GelenByteIndex = 0;
                    KayitYap();
                    SonVeriZamani = DateTime.Now;
                    if (buffer[7] == 9)
                    {
                        byte[] ACK_Byte = new byte[] { 1, 1, 1};
                        SerialPort.Write(ACK_Byte, 0, 3);
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
                }
            }
        }

        public static void StartListening()
        {
            if (SerialPort != null)
            {
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
    }
}

