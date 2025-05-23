# GhostRadio Project
When I was a kid, I was often working with my dad out in the garage, or his shop, or outside. No matter where we were, however, he always kept an old portable transistor radio on him. I didn't love the work, or his choice in music (oldies), but I do maintain a sort of nostalgia for those simpler times. 

I also miss the feeling of trying to track down music in a sea of static. Particularly in AM frequencies, there was no telling what you might find. Especially late at night, when you might chance upon some garbled voice you couldn't quite make out... it was eerie, creepy. Was that a ghost in the radio, trying to make contact across the airwaves?

I want to reproduce that nostalgia. So I bought an old 1960's transistor radio on eBay, ripping out most of the electrical components inside. These old "lunchbox" style radios make it particularly easy, as the entire back is just a panel that you can unbutton and swing open to look at the internals. 

## So what's the point?
The goal is to have an oldschool analog radio that mostly still functions as one. However, I'm not interested in listening to the music you would normally find on AM and FM stations. I like ambient music, video game music, lofi, soundscapes, etc.

So instead, when you turn the dial and search through static, instead you get online radio stations!

That part I've already gotten working, albeit a little crudely.

I could stop there, as the basic functionality is incredibly convenient -- no clicking around through websites, music player interfaces, spammed with ads and whatnot. I just turn it on, turn the dial, and I'm listening to great tunes. I also find I'm a little less assaulted with indecision.

## The other things it will do
What I also want to do, however, is make the radio "haunted." I want spooky stuff to occasionally happen while between stations. Not all the time, mind you. If it happened all the time, it'd lose its novelty. Just once in a great while.

Ghost voices, spooky soundscapes... perhaps at times, the radio even screams at you and shakes!

## Assembly
I'm an absolute novice when it comes to electronics, so keep in mind that my approach is pretty crude. There's been a lot of trial and error (at one point, even had to buy another radio to scavenge more parts. My soldering is messy, cord management is minimal.

But I'm having fun! And learning a lot as I go.

After desoldering and ripping out the majority of the electrical components, what I did leave was the volume knob/potentiometer, the assembly for the tuner, and various switches (on/off, etc.). Basically, the inputs, which I would repurpose.

I got hold of a Raspberry Pi 4, as well as an MCP3008 chip to read analog inputs. After getting familiar with how to wire up and use the chip, I got a little annoyed with how many connections it required even to just read one single analog input, so I soldered together my own "hat" (it doesn't technically qualify as a hat) to plug in on top of the pi.

I also got a tiny rectangular USB speaker. It sounds awful. The pi's not always happy with it. But we don't need to get fancy here -- it just needs to sound about on par with what my dad had.

The pi really only reads two analog signals right now. One from the volume knob, one from a sliding potentiometer (which was supposed to be linear, but definitely isn't). The sliding pot's lever pokes through the old circuit board of the radio, lashed to the tuning needle so it moves along with it (I told you this was crude!).

It's' janky as hell, but it all works!

## Code
I really wanted to do the programming in my language of choice, C#, but I just couldn't get it to read the MCP3008 chip, so I eventually resorted to Python, which I'd done my initial testing with. Maybe one day I'll try again with C#, but for now, you get my sloppy code while I just throw random bullshit at each obstacle I run into.

The volume potentiometer, being so old, is pretty obviously damaged, so I don't get smooth signals from it. And I don't mean it's logarithmic (I mean, it is that, too), I mean it behaves almost like an S-curve in how it reads values. So the code has to account for that.

As for the "linear" tuner potentiometer, I didn't feel like normalizing the values, and I knew I was just going to resort to a dictionary of ranges for handling radio stations anyway. So I just left it at that.

Stations reside in a json file, keyed on ranges of pot values.

Audio is played/streamed using the VLC python library.

## 3D Printing
This project eventually inspired me to get my own 3D printer to design my own housing for the internal parts (of course, I have lots of other project in mind I'll use the printer for). 

A model for said housing is included in this repo. I'm only just getting my feet wet with modeling in OnShape, so I'm regularly making tweaks to it. What's there now works well enough, though.

## What comes next?
Other things I want to add:
- Nice crossfading between static and radio stations.
- Install a rumble motor to get the whole radio to shake as mentioned.
- Get the on/off switch working. Right now, I just plug/unplug the radio into USB.
- Repurpose the "Voice/Music" toggle to instead toggle between music stations and podcasts.
- Try to get the radio's original backlight to work, to indicate on/off status (and mayble have some spooky flickering effects)
- Carve or burn ominous "runes" into the leather housing of the radio.
- And of course, get all those spooky sounds to play randomly through the radio. Even though that's the easiest part, it'll probably come last.


