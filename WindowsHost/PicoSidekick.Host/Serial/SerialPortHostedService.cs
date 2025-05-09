﻿using Diacritics.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using PicoSidekick.Host.Media;
using PicoSidekick.Host.Models;
using PicoSidekick.Host.Performance;
using PicoSidekick.Host.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoSidekick.Host.Serial
{
    public class SerialPortHostedService : BackgroundService
    {
        private readonly MediaService _mediaService;
        private readonly PerformanceService _performanceService;
        private readonly SettingsService _settingsService;
        private readonly SerialPortStatusService _serialPortStatusService;
        private readonly ILogger<SerialPortHostedService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SerialPortHostedService(
            MediaService mediaService,
            PerformanceService performanceService,
            SettingsService settingsService,
            SerialPortStatusService serialPortStatusService,
            ILogger<SerialPortHostedService> logger)
        {
            _mediaService = mediaService;
            _performanceService = performanceService;
            _settingsService = settingsService;
            _serialPortStatusService = serialPortStatusService;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing...");

            SerialPort port = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (port == null || !port.IsOpen)
                    {
                        port = OpenPort();
                        if (port == null)
                        {
                            await Task.Delay(1000, stoppingToken);
                            continue;
                        }
                    }

                    _serialPortStatusService.SetStatus(new SerialPortStatus(true, port.PortName));

                    var perfReading = _performanceService.Read();
                    var mediaReading = await _mediaService.Read();

                    UpdateRequest updateRequest = CreateUpdateRequest(perfReading, mediaReading);
                    string request = JsonSerializer.Serialize(updateRequest, _jsonSerializerOptions);
                    port.WriteLine(request);

                    HandleCommands(port);
                }
                catch (Exception e)
                {
                    _settingsService.DisableChanges();
                    _serialPortStatusService.SetStatus(new SerialPortStatus(false, null));
                    _logger.LogWarning(e, "Error, continuing...");
                }

                await Task.Delay(500, stoppingToken);
            }
        }

        private UpdateRequest CreateUpdateRequest(PerformanceReading perfReading, MediaReading mediaReading)
        {
            return new UpdateRequest
            {
                Artist = mediaReading.Artist,
                Title = mediaReading.Title,
                IsMediaActive = mediaReading.IsMediaActive,
                IsPlaying = mediaReading.IsPlaying,
                Time = DateTime.Now.ToShortTimeString(),
                UsedCPUPercent = perfReading.Cpu,
                UsedRAMGigabytes = perfReading.UsedRamInGigabytes,
                TotalRAMGigabytes = _performanceService.TotalRamInGigabytes,
                UpdatedSettings = _settingsService.GetUpdatedSettings(),
            };
        }

        private void HandleCommands(SerialPort port)
        {
            if (port.BytesToRead > 0)
            {
                string commandJson = port.ReadLine();
                ScreenCommand screenCommand = JsonSerializer.Deserialize<ScreenCommand>(commandJson, _jsonSerializerOptions);
                if (screenCommand.IsSettings())
                {
                    var settings = new SettingsModel(screenCommand.DevModeEnabled, RestartInUf2Mode: false, screenCommand.Brightness, screenCommand.DisplayRotated);
                    _settingsService.SetCurrentSettingsFromScreen(settings);
                }

                if (screenCommand.IsShutdown())
                {
                    var psi = new ProcessStartInfo("shutdown", "/s /hybrid /t 5")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
            }
        }

        private SerialPort OpenPort()
        {
            string portName = SerialDeviceFinder.GetCircuitPythonDataSerialPortName();
            if (portName == null)
            {
                _settingsService.DisableChanges();
                _serialPortStatusService.SetStatus(new SerialPortStatus(false, null));
                _logger.LogInformation("No compatible Circuit Python serial port found");
                return null;
            }

            var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
            {
                DtrEnable = true
            };
            port.Open();

            _logger.LogInformation("Connected!");
            return port;
        }
    }
}
