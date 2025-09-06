# GhostRadio - C# Version

A C# port of the GhostRadio project for Raspberry Pi, converting an old transistor radio into a streaming internet radio with nostalgic analog controls.

## Hardware Requirements

- Raspberry Pi 3 Model B+ (ARM64) or newer
- MCP3008 analog-to-digital converter
- 2x potentiometers (for tuning and volume)
- 1x momentary switch (for power)
- USB speakers or analog audio output

## Hardware Connections

- MCP3008 Channel 0: Tuner potentiometer
- MCP3008 Channel 1: Volume potentiometer  
- GPIO Pin 17: Power switch (active low with pull-up)
- SPI0: MCP3008 communication

## Software Dependencies

- .NET 8.0 Runtime (ARM64)
- LibVLC (automatically installed via NuGet)
- Hardware access permissions for GPIO and SPI

## Installation

1. Install .NET 8.0 on your Raspberry Pi:
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore
```

2. Clone and build the project:
```bash
git clone <repository>
cd ghostradio
dotnet build -c Release
```

3. Ensure you have a `static.wav` file for between-station audio

## Usage

Run the application with appropriate permissions:
```bash
sudo dotnet run --project GhostRadio.csproj
```

Or run the built executable:
```bash
sudo ./bin/Release/net8.0/linux-arm64/GhostRadio
```

## Configuration

Edit `stations.json` to modify radio stations and their corresponding tuner ranges:

```json
{
  "stations": [
    {
      "min": 34.8,
      "max": 41.2, 
      "url": "https://relay.rainwave.cc/game.mp3"
    }
  ]
}
```

## Features

- Analog tuning control with custom volume curve
- Automatic station switching based on tuner position
- Static audio playback between stations
- Power on/off functionality
- Real-time hardware monitoring
- LibVLC-based streaming for reliable audio playback

## Troubleshooting

- Ensure SPI is enabled: `sudo raspi-config` > Interface Options > SPI
- Grant GPIO permissions or run with sudo
- Verify MCP3008 wiring and SPI connections
- Check that LibVLC is properly installed