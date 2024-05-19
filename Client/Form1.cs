using MjpegProcessor;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ComTypes;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Xml.Linq;
using System.Text;

namespace demoMJPEGCamera
{
    public partial class Form1 : Form
    {
        private MjpegDecoder mjpeg;

        TelegramBotClient botClient = new TelegramBotClient("7195745339:AAEoU9_5RzpuTsqRCtedm7oH0eDe4Y9Q7uo");
        List<long> listaChatsId = new List<long>();
        Queue<Bitmap> queue = new Queue<Bitmap> ();
        private readonly object lockObject = new object();

        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(e.FrameBuffer))
            {
                bmp = new Bitmap(ms);
            }

            System.Drawing.Image newImg = (System.Drawing.Image)bmp.Clone();
            bmp.Dispose();

            newImg.RotateFlip(RotateFlipType.RotateNoneFlipX);

            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newImg);
            string drawString = "camera!";
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 12);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);

            var x = 560.0f;
            var y = 460.0f;
            gr.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Green), new System.Drawing.Rectangle(545, 465, 10, 10));

            gr.DrawString(drawString, drawFont, drawBrush, x, y);

            gr.DrawString(DateTime.Now.ToLocalTime().ToString(), drawFont, drawBrush, 12, y);

            if (step == 0)
            {
               // pictureBoxSmart.Image = newImg;
                ConsumeAI(e.FrameBuffer);
                step = 10;
            }
            step--;
            drawFont.Dispose();
            drawBrush.Dispose();
            gr.Dispose();
        }

        private void mjpeg_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }


        public static int step = 0;


        async void ConsumeAI(byte[] jpeg)
        {
            try
            {
                using (var httpClient = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(jpeg);
                    imageContent.Headers.Add("Content-Disposition", "form-data; name=\"image\"; filename=\"gatto.jpeg\"");
                    formData.Add(imageContent);

                    var response = await httpClient.PostAsync("http://localhost:5000/v1/object-detection/yolov5", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        dynamic json = JsonConvert.DeserializeObject(responseData);
                        showSmartImage(json, jpeg);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConsumeAI: {ex.Message}");
            }
        }

        public int persons = 0;
        public int actualFrame = 0;

        async void showSmartImage(dynamic json, byte[] bytes)
        {
            persons = 0;
            MemoryStream stream = new MemoryStream(bytes);
            Bitmap bmp = new Bitmap(stream);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
            foreach (var item in json)
            {
                decimal xmin = item["xmin"];
                decimal xmax = item["xmax"];
                decimal ymin = item["ymin"];
                decimal ymax = item["ymax"];
                decimal confidence = item["confidence"];
                int class2 = item["class"];
                string name = item["name"];
                if (name == "person")
                {
                    persons++;
                }
                response data = new response(xmin, xmax, ymin, ymax, confidence, class2, name);
                gr.DrawString(name, new Font("Arial", 12, FontStyle.Bold), new SolidBrush(System.Drawing.Color.Red), (float)xmin, (float)ymin);
                Rectangle rect = new Rectangle(int.Parse(data.xmin.ToString()), int.Parse(data.ymin.ToString()), int.Parse((data.xmax - data.xmin).ToString()), int.Parse((data.ymax - data.ymin).ToString()));
                gr.DrawRectangle(new Pen(System.Drawing.Color.Red), rect);
            }
            pictureBoxSmart.Image = bmp;
            gr.Dispose();
            actualFrame++;
            Console.WriteLine(actualFrame);
            if (persons >= numericUpDown1.Value && actualFrame >= 30)
            {
                queue.Enqueue(bmp);
                actualFrame = 0;
            }
        }


        public void refreshLbox(ListBox lbox)
        {
            lbox.Items.Clear();
            foreach(var client in listaChatsId)
            {
                lbox.Items.Add(client);
            }
        }

        public async Task StartReceiver()
        {
            var token = new CancellationTokenSource();
            var canceltoken = token.Token;
            var ReOpt = new ReceiverOptions { AllowedUpdates = { } };
            await botClient.ReceiveAsync(OnMessage, ErrorMessage, ReOpt, canceltoken);
        }

        public async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellation)
        {
            Console.WriteLine("Messaggio Ricevuto");
            if (update.Message is Telegram.Bot.Types.Message message)
            {
                try
                {
                    if (!listaChatsId.Contains(message.Chat.Id))
                    {
                        listaChatsId.Add(message.Chat.Id);
                        if (!System.IO.File.Exists("subscrivers.json"))
                        {
                            using (var fs = System.IO.File.Create("subscrivers.json"))
                            {
                                fs.Close();
                            }
                        }
                        if (listaChatsId.Count > 0)
                        {
                            string jsonString = JsonConvert.SerializeObject(listaChatsId, Formatting.Indented);
                            
                            System.IO.File.WriteAllText("subscrivers.json", jsonString);

                            //StreamWriter writer = new StreamWriter("subscrviers.json");
                            //foreach(var item in listaChatsId)
                            //{
                            //    writer.WriteLine(item);
                            //}
                            //writer.Close();
                            Console.WriteLine("Lista Aggiornata");
                        }
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Benvenuto!");
                        refreshLbox(listBox1);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Sei Già Iscritto!");
                    }
                }
                catch
                {
                    MessageBox.Show("Errore Telegram");
                }
            }
        }

        public async Task ErrorMessage(ITelegramBotClient telegramBot, Exception e, CancellationToken cancellation)
        {
            if (e is ApiRequestException)
            {
                await botClient.SendTextMessageAsync("", e.Message.ToString());
            }
        }



        protected void StartMJPEGserverProcess()
        {

            // ffmpeg -list_devices true -f dshow -i dummy
            // ffmpeg - f dshow - list_options true - i
            // video = "Logi C310 HD WebCam"
            // AUKEY PC-W1
            // USB2.0 VGA UVC WebCam





            var camera = "USB2.0 VGA UVC WebCam";
            string arguments = $@"-- ffmpeg  -f dshow -i video=""{camera}"" -video_size 840x680 -framerate 20 -threads 6 -f mpjpeg -r 30 -q 2 -";
            //string arguments = $@"-- ffmpeg -i http://192.168.112.57:81/videostream.cgi?loginuse=admin&loginpas=  -video_size 640x480 -vcodec mjpeg -rtbufsize 1000M -f mpjpeg -r 10 -q 3 -";
            //string arguments = $@"-- ffmpeg -i http://83.56.31.69/mjpg/video.mjpg  -video_size 640x480 -vcodec mjpeg -rtbufsize 1000M -f mpjpeg -r 10 -q 3 -";
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "MJPEGServer.exe",
                Arguments = arguments,
                UseShellExecute = true, // window
                LoadUserProfile = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                //RedirectStandardOutput = true,
            };

            Process.Start(info);
        }

        public Form1()
        {
            InitializeComponent();

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "START") // AVVIA
            {

                StartMJPEGserverProcess(); // avvia lo streaming da ffmpeg

                mjpeg = new MjpegDecoder();
                mjpeg.FrameReady += mjpeg_FrameReady;
                mjpeg.Error += mjpeg_Error;

                mjpeg.ParseStream(new Uri("http://127.0.0.1:9000")); // start stream
                (sender as Button).Text = "STOP";
                pictureBoxSmart.Visible = true;
            }
            else // FERMA
            {

                Process[] processes = Process.GetProcessesByName("MJPEGServer");
                Array.ForEach(processes, (process) =>
                {
                    process.Kill();
                });

                mjpeg.FrameReady -= mjpeg_FrameReady;
                mjpeg.Error -= mjpeg_Error;
                mjpeg.StopStream();

                (sender as Button).Text = "START";
                pictureBoxSmart.Visible = false;
                pictureBoxSmart.Image = null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadStart start = new ThreadStart(async delegate
            {
                await StartReceiver();
            });
            Thread thread = new Thread(start);
            thread.Start();
            if (System.IO.File.Exists("subscrivers.json"))
            {
                string jsonString = System.IO.File.ReadAllText("subscrivers.json");
                listaChatsId = JsonConvert.DeserializeObject<List<long>>(jsonString);
                Console.WriteLine(listaChatsId);
            }
            refreshLbox(listBox1);

            ThreadStart start2 = new ThreadStart(delegate
            {
                while (true)
                {
                    Bitmap img = null;
                    lock (lockObject) // Lock per accedere alla coda
                    {
                        if (queue.Count > 0)
                        {
                            img = queue.Dequeue(); // Rimuove l'immagine dalla coda in modo sicuro
                        }
                    }

                    if (img != null)
                    {
                        try
                        {
                            foreach (var id in listaChatsId)
                            {
                                ThreadStart startID = new ThreadStart(delegate
                                {
                                    MemoryStream ms = new MemoryStream();
                                    lock (lockObject) // Lock per utilizzare l'immagine
                                    {
                                        img.Save(ms, ImageFormat.Bmp); // Salva l'immagine nel MemoryStream
                                    }
                                    ms.Position = 0;
                                    Telegram.Bot.Types.InputFileStream file = new InputFileStream(ms);
                                    botClient.SendTextMessageAsync(id, $"{persons} Persone Rilevate!");
                                    Thread.Sleep(2000);
                                    botClient.SendPhotoAsync(id, file);
                                });
                                new Thread(startID).Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Errore durante l'invio dell'immagine: " + ex.Message);
                        }
                    }
                    Thread.Sleep(500);
                }
            });
            new Thread(start2).Start();

        }
    }
}