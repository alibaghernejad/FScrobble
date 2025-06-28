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

### Last.fm (Step-by-Step initialization):

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
| **Scrobbling.PlaybackPollInterval** | Interval for polling playback status (e.g., every 3 seconds). **FScrobble** uses polling in combination with the media player state to ensure accurate scrobbling. This interval strikes a balance between reliability and performance, minimizing the risk of missing tracks while avoiding unnecessary resource usage. It is recommended to use the default value unless specific requirements necessitate a change. Adjusting this value to be too small may lead to performance issues, while setting it too high could result in missed tracks. | `"00:00:03"` |
| **Scrobbling.MprisServicesPollInterval** | Interval for polling MPRIS services (e.g., every 30 seconds). Behind the scenes, **FScrobble** periodically checks for available compatible media players and processes their streams if required. Using a value that is too small may increase CPU usage and negatively impact performance. It is recommended to use a balanced interval to ensure efficient operation. | `"00:00:30"` |
| **Scrobbling.AllowedPlayers** | List of media players allowed for scrobbling (e.g., Spotify, Rhythmbox, Firefox). | `["spotify*", "rhytmbox*", "firefox"]` |
| **Scrobbling.ThresholdFixed** | Defines a fixed duration threshold for scrobbling a track (e.g., 4 minutes). When the length of the currently playing track is known, the application evaluates both `Scrobbling.ThresholdFixed` and `Scrobbling.ThresholdFraction` rules, applying the first condition that is satisfied. For example, if the track's length is 8 minutes, it will be scrobbled if either 4 minutes (`Scrobbling.ThresholdFixed`) or 50% of the track duration (`Scrobbling.ThresholdFraction`) has been played. Ensure this value aligns with the scrobbling rules of the configured server before making changes.| `"00:04:00"`                   |
| **Scrobbling.ThresholdFraction** | Fractional threshold for scrobbling (e.g., 50% of track duration). This option works in combination with `Scrobbling.ThresholdFixed`. For more information, see the description of `Scrobbling.ThresholdFixed`.          | `0.5`                          |
| **Scrobbling.ThresholdMin** | Defines the minimum duration a track must play before it can be scrobbled (e.g., 1 minute 30 seconds). This helps prevent short audio files, such as ads or jingles, from being scrobbled. The default value is optimized for most scenarios, and changes are generally not recommended unless specific use cases require adjustment. | `"00:01:30"` |
| **LastFm.ApiKey**            | API key for accessing the Last.fm service.                                     | `""` (Empty by default)        |
| **LastFm.ApiSecret**         | API secret for authenticating with the Last.fm service.                        | `""` (Empty by default)        |
| **LastFm.BaseUrl**           | Base URL for the Last.fm API (e.g., `https://ws.audioscrobbler.com/2.0/`).      | `"https://ws.audioscrobbler.com/2.0/"` |
| **LastFm.IsEnabled**         | Enables or disables Last.fm integration (e.g., `true`).                        | `true`                         |

**Tip:** Before modifying your preferred configurations, it is highly recommended to create a backup of your current settings. This ensures that you can easily restore your previous setup if needed.
