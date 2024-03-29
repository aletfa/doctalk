<img src="logo.jpg" alt="drawing" width="400"/>
A .Net, AI tool, to Chat about your Docs.
<br/>
<br/>
Example: A folder with all your e-books, from here you run a technical chat with the AI.<br/>
Example: A folder with documentation of your next work activity, from here perform an AI-assisted discovery.

## Introduction
DocTalk is an orchestration of 3 principal components:

 - Whisper AI
 	 - Thanks to OpenAI Whisper for the algorithm [@officialsite](https://openai.com/research/whisper) 
	 - Thanks to the WhisperNet team [@github](https://github.com/sandrohanea/whisper.net)
  - LLama AI
	  - Thanks to Meta LLama2 for the algorithm [@officialsite](https://llama.meta.com/)
	  - Thanks to TheBlokeAI for the model [@officialsite](https://www.patreon.com/TheBlokeAI)
	  - Thanks to LLamaSharp Team for the .Net wrapper [@github](https://github.com/sandrohanea/whisper.net)
  - Microsoft.KernelMemory [@github](https://github.com/microsoft/kernel-memory)

All to generate what is intended to be **an example** of how it is possible **to exploit the advantages of generative AI** in order **to start a discussion regarding your documentation with an AI assistant**.

All locally and without taking the advantages of the Cloud. This only to avoid getting into difficult problems due to the privacy of the information contained.

## Getting started

 1. Build the project and restore the NuGet packages.
 2. Download [FFMpeg](https://github.com/BtbN/FFmpeg-Builds/releases) and put all the 3 executables in the `bin` folder into the application root directory (example: `DocTalk\bin\Debug\net8.0`)
 3. Start the program
 4. The program ask you for a directory to discuss about
 5. The program will start downloading the AI models
	 - ℹ️: This operation may take some time (more info into the "tweak" section below)
6. All the media file will be read using Whisper AI and converted to text format
7. The LLama model is loaded and the chat with the AI begins.

## Tweak

- **LLama model**

	The model used is the one provided by TheBlokeAI on [hubbingface](https://huggingface.co/TheBloke/Llama-2-7B-Chat-GGUF/tree/main) and in particular the `llama-2-7b-chat.Q4_K_M.gguf` approximately 4 GB.
This because in my opinion, it is the best cost (computational) / benefit ratio.

- **LLamaSharp speedup**

	For compatibility the project starts with the `LLamaSharp.Backend.Cpu` library which uses the CPU for computation. For better performance, if you have an Nvidia RTX video card you can use the CUDA suite which uses the GPU for calculations instead.
1. `Install` the CUDA Toolkit available at [Nvidia](https://developer.nvidia.com/cuda-downloads)
2. `Remove` the `LLamaSharp.Backend.Cpu` NuGet package
3. `Install` the `LLamaSharp.Backend.Cuda12` package (or 11 for backwards compatibility)

- **Move to Cloud**
  
	`Microsoft.KernelMemory` offers various computing options via Cloud, all of which are more efficient than what can be done locally. However, be careful because  the contents of the documents will be sent to the Cloud for analysis. In many scenarios this is completely safe but it is worth evaluating on a case-by-case basis.

## Known Issues

 - `LLamaSharp.kernel-memory` always use the `Console` to log.
 
 - Some AI responses may not always be great. The model used is an previous version of LLama. The latest version also uses 47GB models and offers the best performance ([BlokeAI@hubbingface](https://huggingface.co/TheBloke/Llama-2-70B-Chat-GGUF/tree/main))


## Special Thanks
- Martin Evans ([@martindevans](https://github.com/martindevans)) for his quickly support about LLamaSharp.
- Marco Minerva ([@marcominerva](https://github.com/marcominerva)) for his always useful support and for the quality of the code he publishes.
- Copilot ([@officialsite](https://copilot.microsoft.com/)) for the logo and the support during the coding sessions.
