using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class CameraWebServer
{
    static Process? libcameraProcess;

    static void Main()
    {
        string dossierStream = "/home/pi/stream";
        if (!Directory.Exists(dossierStream))
            Directory.CreateDirectory(dossierStream);

        StartLibcamera();

        HttpListener serveur = new HttpListener();
        serveur.Prefixes.Add("http://*:8080/");
        serveur.Start();

        string ip = GetLocalIPv4() ?? "localhost";
        Console.WriteLine($"Serveur lancé sur http://{ip}:8080");

        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Arrêt du serveur...");
            StopLibcamera();
            serveur.Stop();
            Environment.Exit(0);
        };

        while (true)
        {
            try
            {
                var context = serveur.GetContext();
                var request = context.Request;
                var response = context.Response;

                string urlDemandee = request.RawUrl ?? "/";
                if (urlDemandee == "/")
                    urlDemandee = "/index.html";

                string cheminFichier = Path.Combine(dossierStream, urlDemandee.TrimStart('/'));

                if (urlDemandee.EndsWith(".html"))
                {
                    string html = @"<html><body>
                    <h1>Stream caméra</h1>
                    <img src='/output.mjpeg' />
                    </body></html>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
                    response.ContentType = "text/html";
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else if (File.Exists(cheminFichier))
                {
                    string extension = Path.GetExtension(cheminFichier);
                    response.ContentType = extension switch
                    {
                        ".mjpeg" => "image/jpeg",
                        ".ts" => "video/MP2T",
                        ".m3u8" => "application/vnd.apple.mpegurl",
                        _ => "application/octet-stream"
                    };

                    using var fs = File.OpenRead(cheminFichier);
                    fs.CopyTo(response.OutputStream);
                }
                else
                {
                    response.StatusCode = 404;
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }
    }

    static string? GetLocalIPv4()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ip in ipProps.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip.Address))
                {
                    return ip.Address.ToString();
                }
            }
        }
        return null;
    }

    static void StartLibcamera()
    {
        libcameraProcess = new Process();
        libcameraProcess.StartInfo.FileName = "libcamera-vid";
        libcameraProcess.StartInfo.Arguments = "-t 0 --codec mjpeg -o /home/pi/stream/output.mjpeg";
        libcameraProcess.StartInfo.UseShellExecute = false;
        libcameraProcess.StartInfo.CreateNoWindow = true;
        libcameraProcess.StartInfo.RedirectStandardError = true;
        libcameraProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine("[libcamera-vid] " + e.Data);
        };
        libcameraProcess.Start();
        libcameraProcess.BeginErrorReadLine();
        Console.WriteLine("libcamera-vid lancé");
    }

    static void StopLibcamera()
    {
        if (libcameraProcess != null && !libcameraProcess.HasExited)
        {
            libcameraProcess.Kill();
            libcameraProcess.Dispose();
            Console.WriteLine("libcamera-vid arrêté");
        }
    }
}
