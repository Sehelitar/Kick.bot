# Kick.com integration for Streamer.bot

This integration for Streamer.bot is very "raw" for the moment but covers all the basics.
You will get :

* Custom events triggers for all basic streaming events : new followers, subs, resubs, gifts and even moderation
events and messages events.
* 2 C# methods to send messages and replies to your chat.
* Support for Streamer.bot commands ! Yes, the ones from the "Commands" tab.

Just take a look to the triggers in ``Custom`` => ``Kick`` and see by yourself :)

**Contact**

[![Kick](https://play-lh.googleusercontent.com/66czInHo_spTFWwLVYntxW8Fa_FHCDRPnd3y0HT14_xz6xb_lqSv005ARvdkJJE2TA=s32-rw)](https://kick.com/sehelitar)
[![Twitch](https://play-lh.googleusercontent.com/QLQzL-MXtxKEDlbhrQCDw-REiDsA9glUH4m16syfar_KVLRXlzOhN7tmAceiPerv4Jg=s32-rw)](https://twitch.tv/sehelitar)
[![Twitter/X](https://play-lh.googleusercontent.com/XyI6Hyz9AFg7E_joVzX2zh6CpWm9B2DG2JuEz5meCFVm4-wTKTnHgqbmg62iFKe4Gzca=s32-rw)](https://twitter.com/sehelitar)
[![Youtube](https://play-lh.googleusercontent.com/lMoItBgdPPVDJsNOVtP26EKHePkwBg-PkuY9NOrc-fumRtTFP4XhpUNk_22syN4Datc=s32-rw)](https://youtube.com/@sehelitar)

### -- WARNING --

Compatible only with Streamer.bot **> 0.2.0**

## Compilation

To fully compile this extension from scratch, you need to have access to my Kick API implementation (added as a
submodule in ``KickAPI``). That implementation, based on the actual private API, is in a private repository.
I could make it public, but since this is a private API and the public one should be up soon, I made the choice to
not release it. This extension will use the new public API as soon as possible, and all sources will be released then.

However, all release packages come with a compiled version of Kick.dll you can use as a reference in this project,
so you actually CAN compile it that way.

## Installation

To install this extension, download the latest release available and copy all files into ``dlls`` folder of your
Streamer.bot installation.
Then, import ``actions.txt`` into Streamer.bot using the Import button in the top toolbar of the app.

If the installation is successful, a browser window will open. If not, check if the code in the imported action
can compile successfuly and have no missing references. All required references are in the ``dlls`` folder of SB,
no other download is necessary.

You just have to authenticate yourself on Kick, the browser window will close by itself once it's done.
A Windows notification will appear to confirm your authentication is successful.

## Usage

Just add Kick triggers to your actions and see for yourself !

All triggers mimic Twitch events (as far as I could), so Twitch documentation for the corresponding triggers applies.
(see https://wiki.streamer.bot/en/Platforms/Twitch/Events)

And for both C# methods, some arguments are required to be set before calling any of them :

### SendMessage

| Argument | Type   | Value  |
| :----- | :----- | :----- |
| message | string | The message you want to send. |

### SendReply

| Argument | Type   | Value  |
| :----- | :----- | :----- |
| reply | string | The message you want to send. |
| message | string | Content of the message you are replying to. |
| msgId | string | Id of the message you are replying to. |
| user | long | Username of the user you are replying to. |
| userId | string | Id of the user you are replying to. |

Note : If you reply to a message in an action that was called by a message/command trigger, all these arguments but
your reply will already be set.

## Bugs

Feel free to open an issue on Github if you have a problem. Please provide a maximum of informations, including
bot logs and instructions to reproduce your problem.

## License

This project is distributed under MIT License.