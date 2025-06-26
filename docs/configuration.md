# 🛠 Configuration
FScrobble requires linking to at least one scrobbling server to function properly.  
Running the application without configuring a server will result in no activity or functionality.  

**Important:** FScrobble does not automatically scrobble musics for all MPRIS-compatible media players on your system. This is a deliberate design choice to protect your privacy. It is strongly recommended to explicitly allow only the media players you trust. 
Additionally, FScrobble provides the flexibility to define metadata rules for each allowed media player, enabling fine-grained control over what gets scrobbled. 

## Supported Scrobbling Servers:

- [x] ![Last.fm](assets/lastfm-icon-48x48.png) **Last.fm** (Supported)
- [ ] ![Libre.fm](assets/librefm-icon-48x48.png) **Libre.fm** (Not Implemented yet)
- [ ] ![ListenBrainz](assets/listenbrainz-icon-48x48.png) **ListenBrainz** (Not Implemented yet)
- [ ] ![Maloja](assets/maloja-icon-32x32.png) **Maloja** (Not Implemented yet)

## Step-by-Step Workflow:

1. Obtain your **API Key** and **Secret** from [Last.fm](https://www.last.fm/api/account/create). Ensure you have a Last.fm account to access these credentials.
To configure your Last.fm API keys, use environment variables:
2. Update the `settings.json` file or set the following environment variables with your credentials:

   **Using Environment Variables:**
   ```bash
   export LastFm__ApiKey=YOUR_API_KEY
   export LastFm__ApiSecret=YOUR_API_SECRET
   ```

   **Using `settings.json`:**
   Create or update a `settings.json` file in the application's configuration directory with the following structure:
   ```json
   {
     "LastFm": {
       "ApiKey": "YOUR_API_KEY",
       "ApiSecret": "YOUR_API_SECRET"
     }
   }
   ```
 3. Initialize the connection to **Last.fm** by running the following command and following the on-screen instructions:
      ```bash
      cd FScrobble/FScrobble.Shell
      dotnet run connect lastfm
      ```
4. If the setup is successful, you will see something like this message:
   _"Connection to .. scrobbling server successfully initialized."_
5. After completing the setup, close the program and start it in normal mode:
   ```bash
   dotnet run
6. Verify that scrobbling is working by visiting your scrobbling dashboard and checking if your recently played tracks are being recorded.

By default, FScrobble allows scrobbling for a limited set of MPRIS-compatible media players to ensure user privacy and security. 

## Default Allowed Media Players:
   - [**Musikcube**](https://musikcube.com): A lightweight CLI-based media player.
   - [**YouTube Music**](https://music.youtube.com): A popular music streaming service.

## Supported Configs
Here are the full list of supported configuration by **FScrobble**.

Here is an updated table with a column for default values:

| **Configuration Key**       | **Description**                                                                 | **Default Value**               |
|------------------------------|---------------------------------------------------------------------------------|---------------------------------|
| **Logging.LogLevel.Default** | Sets the default logging level (e.g., Information).                            | `"Information"`                |
| **Logging.LogLevel.Microsoft.Hosting.Lifetime** | Configures logging for the Microsoft hosting lifetime.                  | `"Information"`                |
| **Scrobbling.PlaybackPollInterval** | Interval for polling playback status (e.g., every 3 seconds).             | `"00:00:03"`                   |
| **Scrobbling.MprisServicesPollInterval** | Interval for polling MPRIS services (e.g., every 30 seconds).            | `"00:00:30"`                   |
| **Scrobbling.AllowedPlayers** | List of media players allowed for scrobbling (e.g., Spotify, Rhythmbox, Firefox). | `["spotify*", "rhytmbox*", "firefox"]` |
| **Scrobbling.ThresholdFixed** | Fixed threshold for scrobbling a track (e.g., 4 minutes).                      | `"00:04:00"`                   |
| **Scrobbling.ThresholdFraction** | Fractional threshold for scrobbling (e.g., 50% of track duration).           | `0.5`                          |
| **Scrobbling.ThresholdMin** | Minimum threshold for scrobbling a track (e.g., 1 minute 30 seconds).           | `"00:01:30"`                   |
| **LastFm.ApiKey**            | API key for accessing the Last.fm service.                                     | `""` (Empty by default)        |
| **LastFm.ApiSecret**         | API secret for authenticating with the Last.fm service.                        | `""` (Empty by default)        |
| **LastFm.BaseUrl**           | Base URL for the Last.fm API (e.g., `https://ws.audioscrobbler.com/2.0/`).      | `"https://ws.audioscrobbler.com/2.0/"` |
| **LastFm.IsEnabled**         | Enables or disables Last.fm integration (e.g., `true`).                        | `true`                         |

This table now includes the default values for each configuration key.