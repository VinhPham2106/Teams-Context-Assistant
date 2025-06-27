# MS Intern Hackathon 2025: Teams-Context-Assistant

## Introduction
This project is a Proof-of-Concept of providing real-time update on the topic currently going on in a meeting, different leads and links for you to quickly follow-up. It's an add-on to the already powerful live transcript feature Teams + Copilot provides, which adds customizable integration that fits your team, whether it be your Action Items, Glossary, Onboarding Guide. It's a much better alternative to waiting on the meeting to end, the transcript to load and for Copilot to summarize it. You would want those updates in real time, where you can act on it while not being distracted trying to search up.

For a full story and description, check out our [Hackathon Project Page](https://innovationstudio.microsoft.com/hackathons/Intern-Hackathon-2025/project/95466)

## Testing
There are 2 methods to run this PoC, using a one docker-compose command or running the 2 programs locally.

Regardless of the method, you will need an OPENAIKEY. Put an `.env` file in the "BotIntelligenceEmulator" folder, something like `OPENAI_API_KEY=sk-proj-`

For customization of Teams' transcript, simply edit "TranscriptEmulator/defaultTranscript.json"

### 1. Docker-Compose 
Clone the repo and run this command:
```
docker compose up --build -d
```

Then check the terminal output of both services

### 2. Locally with Visual Studio (Code)
It's best to use Visual Studio. Simply go into "BotIntelligenceEmulator" and run the `Program.cs`, then head over to "TranscriptEmulator" and do the same. Observe the output from both Command Line output. 

## Reason to Emulate
Right now Teams have a Live transcript feature, but it isn't exposed externally (eg. Graph API). The way we envision to get it is to have our Teams bot sit in the meeting, stream audio frames to Azure Cognition Service for its Speech-To-Text feature, then stream it to some kind of Copilot Intelligence. 

For the sake of demoing, we're abstracting it with a WebSocket setup that chunks a given transcript to batch of text that mimics a buffer of processed audio frames mentioned above.

For the bot, we decided to focus on demoing the functionality (there are nuances for the bot, such as permission by Tenant Admins to allow to bot to sit in the call, record, or deploying the bot on Azure). In future work, we plan to actually involve the BotSDK, create a Tenant, set up the permissions, have it ready to deploy. It would be a fun thing to switch on to whenever we're blocked at work

## Future work
- Unit and Integration Testing
- Integrating with BotSDK and deploy
- Set up a test Tenant with Copilot subscription and configured permissions for the bot app


