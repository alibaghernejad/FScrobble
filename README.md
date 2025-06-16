# FScrobble
Welcome to **FScrobble**, A Music Scrobbling deamon service, written in F# and .NET.   
FScrobble aims to be a _simple, reliable and efficient_ D-Bus and MPRIS compatible music scrobbler service,    
allowing users to track their **music listening habits** by automatically submitting track information to desired servers.   

<img src="assets/fscrobble.png" alt="FScrobble Logo" width="33.3%" />

"Built with ❤ using F# and .NET, with a focus on reliability and efficiency."  


## 🚀 Features

- 🎵 **Real-time Scrobbling**: Automatically track your music listening habits.
- 🔗 **Last.fm Integration**: Connect your Last.fm account effortlessly.
- ⚡ **Lightweight and Fast**: Built with F# for performance, simplicity and readability.
- 🌐 **D-Bus and MPRIS-Compatible**: Works well for standard media players like Musikcube, YouTube Music,.. on Linux


## 📦 Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/FScrobble.git
   ```
2. Navigate to the project directory:
   ```bash
   cd FScrobble
   ```
3. Build and run the application:
   ```bash
   dotnet run
   ```


## 🛠 Configuration

To configure your Last.fm API keys, use environment variables:

```bash
export LastFm__ApiKey=YOUR_API_KEY
export LastFm__ApiSecret=YOUR_API_SECRET
```

Alternatively, you can create an `appsettings.Production.json` file for user overrides.


## 💡 Why F# and .NET?

To be honest this a side project. A hobby project. During listenning to the _[Udo Lindenberg](https://en.wikipedia.org/wiki/Udo_Lindenberg)_ and [Bruce Springsteen](https://en.wikipedia.org/wiki/Bruce_Springsteen) **Catchy Songs**, needed to track my _music listening habits_ and came to this idea to write my own script, using MPRIS.
Obviously there are another services that are great and can do that but i wanted to have a full control about the the data that transmitted to the provided servers.
Alongside that, using F# echo-system and it's unique approach to software development that brings functional programming in combine to .NET that seamlenslyy improved by performace, security and features in new versions was a great experience.
It's not about F#. It's about this fact that there are another approachs to software development, in which the result product is efficent, easier to develop, deploy and maintenance.


## Name Candidates
As you know **Naming the things** are very important by **Software engineers and developers**! There was a few option and consideration for naming the project.  
At the first i was chosen the  **Nur** + **Scrobbling** pairs. That is **NurScrobbling**. Not Bad!  
_Nur_ is a German word means _only_ in English and indicates that this
service is responsible for scrobbling the musics only. After that i came to the _NurScrobble_ that was succinct, versatile, and  easier to remember.
All in one, i decided to use **FS prefix** as a indicator that this project has been written in **Fsharp**. The second charachter by FS -S- was also match with the Next word _Scrobble_. So the **FScrobble** not only put the **Fsharp and Scrobbling** into the context, but also sounds elegant and chick (hopefully!) 


## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.


## 🤝 Contributing

We welcome contributions! Feel free to open issues, submit pull requests, or suggest new features.
Keep in mind that there are room for improvements, and there are also known issues that will be fixed for next releases.


## 📧 Contact

For questions or feedback, reach out to us at [alibaghernezhad@gmail.com](mailto:aliaghernezhadl@gmail.com).


¹ **Footnote**: The pipe operator (`|>`) is a core feature of F# that allows you to pass the result of one function as an argument to another, enabling clean and readable functional code.

