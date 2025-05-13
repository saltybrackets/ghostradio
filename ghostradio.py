import vlc
import time
import math
import json
from gpiozero import MCP3008
from gpiozero import Button

# Initialize MCP3008 channels
tuner = MCP3008(channel=0)
volume_knob = MCP3008(channel=1)
power_switch = Button(17, pull_up=True)

# Path to local static file (should be in same directory)
STATIC_FILE = "static.wav"

# Load station tuning map from JSON file
def load_station_map(filepath="stations.json"):
    with open(filepath, "r") as f:
        data = json.load(f)
    return [((entry["min"], entry["max"]), entry["url"]) for entry in data]

# Power switch
def read_power_switch():
    return power_switch.is_pressed

# Read analog values as 0–100%
def read_analog_percentage(channel):
    return channel.value * 100.0

def read_inverted_analog_percentage(channel):
    return 100.0 - (channel.value * 100.0)

# Volume curve: gentle dropoff below 90, hard mute below ~30
def read_volume_percentage(channel):
    raw = channel.value
    linear_volume = raw * 100.0

    if linear_volume >= 90.0:
        return linear_volume
    else:
        scaled = (linear_volume / 90.0) ** 0.15
        scaled *= 90.0
        if scaled < 30:
            return 0.0
        return scaled

# Match tuner value to a station URL
def get_station_url(tuner_value, station_map):
    for (low, high), url in station_map:
        if low <= tuner_value <= high:
            return url
    return None

# Load stations from file
station_map = load_station_map()

# Set up VLC player
instance = vlc.Instance()
player = instance.media_player_new()
current_station_url = None
power_was_on = False
is_power_on = False

try:
    while True:
        power_was_on = is_power_on
        tuner_value = read_inverted_analog_percentage(tuner)
        volume_value = read_volume_percentage(volume_knob)
        is_power_on = read_power_switch()

        print(f"Power: {is_power_on}  Tuner: {tuner_value:6.1f}  Volume: {volume_value:6.1f}", end='\r')

        if is_power_on == False:
            player.stop()
            continue

        player.audio_set_volume(int(volume_value))

        station_url = get_station_url(tuner_value, station_map)

        if station_url != current_station_url or power_was_on == False:
            current_station_url = station_url
            player.stop()

            if current_station_url:
                print(f"\nTuning to station: {current_station_url}")
                media = instance.media_new(current_station_url)
            else:
                print("\nNo station matched. Playing static.")
                media = instance.media_new_path(STATIC_FILE)

            player.set_media(media)
            player.play()

        time.sleep(0.05)

except KeyboardInterrupt:
    print("\nExiting...")
    player.stop()
