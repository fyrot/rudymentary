# Rudymentary             <img src="https://raw.githubusercontent.com/fyrot/rudymentary/475c68a965ca44cde4ce133abdbb58522d1f2069/Resources/AppIcon/rudymentarylogoborder.svg" width="24" height="24" margin-left="100" />

*An open-source, lightweight, local music player application developed with Microsoft's C# .NET MAUI framework -- providing support for various platforms including Windows, macOS, Android, and iOS.*

### Maintenance note

This repository likely won't see any updates; instead it'll be left here as a public archive. Rudymentary was a good exercise in not only learning how to use a language (C#) but in going beyond the syntax and actually interacting with some of its more fledgling frameworks (.NET MAUI), and as it names suggests, its UX is undoubtedly quite basic and less flashy. It's definitely been a stepping stone in how I approach not only user-facing design but also in how I wrote the supporting logic for the app -- there's definitely a lot I would have changed and the develeopments I've made in my approach should be way more obvious when looking at some of these later small projects, haha.

For my friends who have been eagerly awaiting a build of this since day one.. uh.. you can try cloning this repository and build it yourself (yay)! This idea was continued and expanded upon in Rhymx ("rhymx-tw"), where the app goes from being built on MAUI to an Electron app, that started incorporating a much more appealing (at least, in my opinion) visual style alongside backend improvements like better music organization, caching, synced lyrics support, and a framework for building "side-loadable" plugins that can execute code and communicate with the app (kind of like modes). See you later!

## Feature list
 - Ability to select various root folders to automatically categorize and index all music (.mp3, .flac) files within that directory
   - Metadata is parsed in order to associate songs by properties like their album name, album art, contributing artists, and more
   - Conditionally rendered lyrics page for lyric metadata
 - Support for synced lyrics (via .lrc file) to have lyrics appear with where they are placed within the currently-playing song 
 - User-generated playlists
 - Intuitive, persistent player bar with various conveniences
   - Displays song name, artist name, and album art (if present/applicable)
   - Dynamic slider to seek a specified position within the playing song (with metrics like current position + total duration)
   - Accessible buttons to skip backward from ( ⏮ ), pause/play, or skip forward from ( ⏭ )the current track
   - Ability to toggle a shuffled or cyclical queue 
   - Volume slider for convenient adjustment
 - Optional user-oriented, personalize-able theming of several components and pages within the app
 - Included "smart" translation/transliteration of lyrics into several languages (provided by Microsoft's Web Translator)
 - Among others

## Screenshots     
<figure>
  <img src="https://github.com/fyrot/rudymentary/assets/142183447/5cb4c8c4-e980-48a4-ac47-006fddb8c131" />
  <figcaption><i>Home page</i></figcaption>
</figure>
<br />
<br />
<figure>
  <img src="https://github.com/fyrot/rudymentary/assets/142183447/fc7095f5-4749-49be-8045-84f9f412c9f5" />
  <figcaption><i>Album page</i></figcaption>
</figure>
<br />
<br />
<figure>
  <img src="https://github.com/fyrot/rudymentary/assets/142183447/2dbdd5d8-ed56-4f1e-9bb7-43ffa4bd28a1" />
  <figcaption><i>Settings/preferences</i></figcaption>
</figure>
<p align="center" >
  <img width="200" height="200" src="https://raw.githubusercontent.com/fyrot/rudymentary/475c68a965ca44cde4ce133abdbb58522d1f2069/Resources/AppIcon/rudymentarylogoborder.svg" />
</p>






*README and screenshots are subject to change -- a work in progress, :)*
