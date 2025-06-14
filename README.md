# FScrobble
**FScrobble** aims to be a _reliable and efficient_ D-Bus and MPRIS compatible music scrobbler service, allowing users to track their _music listening habits_ by automatically submitting track information to desired servers. 

"Built using F# with a focus on reliability and efficiency."  

**F#**, combined with _.NET_, is an excellent choice for system application development.
F# code is efficient, concise, readable, and aesthetically pleasing.
_FScrobble_ is a .NET Worker Service written in F# that runs as a background service.

## Project Structure

This project is built using:
- F# (.NET 9.0)
- .NET Worker Service template
- Microsoft.Extensions.Hosting

## Prerequisites

- .NET 9.0 SDK or later
- F# development tools

## Scrobbling Rules
The most reliable approach for scrobbling follows Last.fm's official guidelines:

1. **Primary Scrobbling Condition**: 
   - When track has been played for at least 50% of its duration OR
   - For tracks longer than 8 minutes, when played for at least 4 minutes

2. **Optimal States for Scrobbling**:
   - `Paused`
   - `Stopped`
   - When switching to a new track while in `Playing` state

## Supported Scrobble Servers

### ![Last.fm](images/lastfm-icon.png) Last.fm

Last.fm is the original and most popular music scrobbling service. It provides:
- Extensive music tracking and statistics
- Personal listening history and reports
- Music recommendations based on listening habits
- Social features and community interaction
- Rich API for developers

## Configuration

### Last.fm Authentication
To use FScrobble with Last.fm, you need to configure the following credentials in `appsettings.json`:

```json
"LastFm": {
  "ApiKey": "your_api_key_here",
  "ApiSecret": "your_api_secret_here",
  "SessionKey": "your_session_key_here"
}
```

To obtain these credentials:
1. Create a Last.fm API account at [Last.fm/api](https://www.last.fm/api/account/create)
2. After creating your API account, you'll receive:
   - API Key
   - API Secret
3. The SessionKey is obtained through the authentication process when you first run the application


future works:
   - repeative playbacks detaction support
   




[MIT License](LICENSE)