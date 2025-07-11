# FScrobble
A Music Scrobbling deamon service, written in F# and .NET.   
**FScrobble** aims to be a _simple, reliable and efficient_ D-Bus and MPRIS music scrobbler service,    
allowing users to track their **music listening habits** by automatically submitting track information to **Allowed servers**.   

<img src="assets/fscrobble-v0.0.2.png" alt="FScrobble Logo" width="35%" />   

"Built with ❤ using F# and .NET."  

¹ **Footnote**: The pipe operator (`|>`) is a core feature of F# that allows you to pass the result of one function as an argument to another, enabling clean and readable functional code. The logo reflects this concept, symbolizing a pipeline of music streams that are meant to be scrobbled.


## Highlights

- 🎵 **Real-time Scrobbling** — Automatically track your music listening habits.
- 🔗 **Last.fm Integration** — Connect your Last.fm account effortlessly.
- ⚡ **Lightweight and Fast** — Built with F# for performance, simplicity and readability.
- 🖥️ **D-Bus and MPRIS-Compatible** — Fully compatible with standard media players like Musikcube, YouTube Music, and others on Linux.


## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/FScrobble.git
   ```
2. Navigate to the project directory:
   ```bash
   cd FScrobble/src/FScrobble.Shell
   ```
3. Build and run the project:
   ```bash
   dotnet run 
   ```


## Configuration
FScrobble requires linking to at least one scrobbling server to function properly.  
Running the application without configuring a server will result in no activity or functionality.  

**Important:** FScrobble <u>does not</u> automatically <u>scrobble</u> music for <u>all MPRIS-compatible media players</u> on your system <u>by default</u>. This is a deliberate design choice to protect your privacy. It is strongly recommended to explicitly allow only the media players you trust. 
Additionally, FScrobble provides the flexibility to define metadata rules for each allowed media player, enabling fine-grained control over what gets scrobbled. 
For more details, refer to the **Configuration** section.


### Supported Scrobbling Servers:

- [x] ![Last.fm](assets/lastfm-icon-48x48.png) **Last.fm** (Supported)
- [ ] ![Libre.fm](assets/librefm-icon-48x48.png) **Libre.fm** (Not Implemented yet)
- [ ] ![ListenBrainz](assets/listenbrainz-icon-48x48.png) **ListenBrainz** (Not Implemented yet)
- [ ] ![Maloja](assets/maloja-icon-32x32.png) **Maloja** (Not Implemented yet)

### Step-by-Step Workflow:

1. Obtain your **API Key** and **Secret** from [Last.fm](https://www.last.fm/api/account/create). Ensure you have a Last.fm account to access these credentials.
To configure your Last.fm API keys, use environment variables:
2. Update the `config.json` file or set the following environment variables with your credentials:

   **Using Environment Variables:**
   ```bash
   export LastFm__ApiKey=YOUR_API_KEY
   export LastFm__ApiSecret=YOUR_API_SECRET
   ```

   **Using `config.json`:**
   FScrobble automatically create `config.json` file with default settings at ` ~/.config/fscrobble/config.json` if it not exists.
   Update _ApiKey_ and _ApiSecret_ of the LastFm file in as showed bellow:
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
      cd  ~/.local/bin/
      fscrobble connect lastfm
      ```
4. If the setup is successful, you will see something like this message:
   _"Connection to .. scrobbling server successfully initialized."_
5. After completing the setup, close the program and restart the FScrobble systemd service.
   ```bash
   systemctl --user restart fscrobble
6. Verify that scrobbling is working correctly by visiting your scrobbling dashboard and checking if your recently played tracks are being recorded.

By default, FScrobble allows scrobbling for a limited set of MPRIS-compatible media players to ensure user privacy and security. 

### Default Allowed Media Players:
These are the default allowed media players rules and can be configured as you wish.  
Two media players are allowed by default. That means the music streams from these media players are tracked for scrobbling:
   - [**Musikcube**](https://musikcube.com): A lightweight CLI-based media player.
   - [**YouTube Music**](https://music.youtube.com): A popular music streaming service. Allowed on Firefox ([Zen-Browser](https://zen-browser.app/))

There are only two allowed media players by default: **Musikcube** and **YouTube Music (on Firefox)**.


## Why F# and .NET?

To be honest, this is a side project—a hobby project. While listening to _[Udo Lindenberg](https://en.wikipedia.org/wiki/Udo_Lindenberg)_ and [Bruce Springsteen](https://en.wikipedia.org/wiki/Bruce_Springsteen)'s **catchy songs**, I felt the need to track my _music listening habits_ and came up with the idea of writing my own script using MPRIS.  
Of course, there are other great services that can do this, but I wanted full control over the data being transmitted to the provided servers.  
Additionally, working with the F# ecosystem and its unique approach to software development—which combines functional programming with .NET—was a fantastic experience. The seamless improvements in performance, security, and features in recent .NET versions made it even better.  
It's not just about F#. It's about exploring alternative approaches to software development, where the resulting product is efficient, easier to develop, deploy, and maintain.


## Name Candidates
As you know, **naming things** is very important for **software engineers and developers**! There were a few options and considerations for naming the project.  
At first, I chose the **Nur** + **Scrobbling** pair, resulting in **NurScrobbling**. Not bad!  
_Nur_ is a German word that means _only_ in English and indicates that this service is responsible for scrobbling music only. After that, I came up with _NurScrobble_, which was succinct, versatile, and easier to remember.  
All in all, I decided to use the **FS prefix** as an indicator that this project is written in **F#**. The second character in FS, **S**, also matches the next word, _Scrobble_. So, **FScrobble** not only puts **F# and Scrobbling** into context but also sounds elegant and chic (hopefully!).


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.


## Contributing

We welcome contributions! Feel free to open issues, submit pull requests, or suggest new features.  
Keep in mind that there is always room for improvement, and there are also known issues that will be addressed in future releases.


## Contact

For questions or feedback, reach out to us at [alibaghernezhad@gmail.com](mailto:aliaghernezhadl@gmail.com).
