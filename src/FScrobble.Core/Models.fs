namespace FScrobble.Core
module Models =

  open System

  type PlaybackHistory =
      Map<string, DateTimeOffset> 

  type TrackInfo =
      { Id: string
        Artist: string list
        Title: string
        Album: string option
        Length: TimeSpan option
        Url: string option
        ReplayCount: uint16
        PlayerId: string
        MetadataMap : Map<string,string>
         }

  type MediaPlayerState =
      { PlaybackStatus: string
        TrackInfo: TrackInfo option
        Position: int64 }

  
  type Playback =
      { CurrentTrack: TrackInfo option
        PlaybackStartTime: DateTimeOffset option
        Position : int64
        // ScrobbleHistory: PlaybackHistory 
      }

  
  type  PlaybackSnapshot =
      { CurrentTrack: TrackInfo //option
        PlaybackStartTime: DateTimeOffset //option
        Position : int64
        EffectivePlayTime: TimeSpan
        // Small spans, between each pool
        // AccumulatedCalc: (DateTimeOffset * DateTimeOffset) (*current timestamp,previous timestamp*)
        // Long spans between transitions
        AccumulatedCalc2: (DateTimeOffset * TimeSpan) (*current timestamp,previous timestamp*)
        MediaPlayerState : MediaPlayerState
      }
      
    
    type EventBus<'msg> =
      { Post : 'msg -> Async<unit>
        PostAndAsyncReply : (AsyncReplyChannel<bool> -> 'msg)  -> Async<bool> 
      }

    type MediaPlayerTrackerMsg =
    | TryAdd of string * AsyncReplyChannel<bool>
    | Remove of string
