﻿using BeetleX;
using BeetleX.EventArgs;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformBenchmarks
{
    public class HttpServer : IHostedService
    {
        public static IServer ApiServer;

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            ArraySegment<byte> date = GMTDate.Default.DATE;
            ServerOptions serverOptions = new ServerOptions();
            serverOptions.LogLevel = LogType.Error;
            serverOptions.DefaultListen.Port = 8080;
            serverOptions.Statistical = false;
            serverOptions.BufferSize = 1024 * 8;
            serverOptions.BufferPoolMaxMemory = 1000;
            serverOptions.BufferPoolSize = 1024 * 8;
            ApiServer = SocketFactory.CreateTcpServer<HttpHandler>(serverOptions);
            ApiServer.Open();
            if (Program.UpDB)
            {
                RawDb._connectionString = "Server=tfb-database;Database=hello_world;User Id=benchmarkdbuser;Password=benchmarkdbpass;Maximum Pool Size=256;NoResetOnClose=true;Enlist=false;Max Auto Prepare=3";
                //RawDb._connectionString = "Server=192.168.2.19;Database=hello_world;User Id=benchmarkdbuser;Password=benchmarkdbpass;Maximum Pool Size=256;NoResetOnClose=true;Enlist=false;Max Auto Prepare=3";
                await DBConnectionGroupPool.Init(256, RawDb._connectionString);
                ApiServer.Log(LogType.Info, null, "init connection pool size:256");
            }
            else
            {
                // RawDb._connectionString = "Server=192.168.2.19;Database=hello_world;User Id=benchmarkdbuser;Password=benchmarkdbpass;Maximum Pool Size=256;NoResetOnClose=true;Enlist=false;Max Auto Prepare=3";
                RawDb._connectionString = "Server=tfb-database;Database=hello_world;User Id=benchmarkdbuser;Password=benchmarkdbpass;Maximum Pool Size=32;NoResetOnClose=true;Enlist=false;Max Auto Prepare=3";
                await DBConnectionGroupPool.Init(32, RawDb._connectionString);
                ApiServer.Log(LogType.Info, null, "init connection pool size:32");
            }
            await UpdateCommandsCached.Init();
            await Task.Delay(5000);
            ApiServer.Log(LogType.Info, null, "init update commands pool!");
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync("http://localhost:8080/json");
            ApiServer.Log(LogType.Info, null, $"Get josn {response.StatusCode}");
            response = await client.GetAsync("http://localhost:8080/plaintext");
            ApiServer.Log(LogType.Info, null, $"Get plaintext {response.StatusCode}");

            ApiServer.Log(LogType.Info, null, $"Init update commands cached");
            ApiServer.Log(LogType.Info, null, $"Debug mode [{Program.Debug}]");


        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            ApiServer.Dispose();
            return Task.CompletedTask;
        }
    }
}
