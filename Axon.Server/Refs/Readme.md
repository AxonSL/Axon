Most of the dependencies can be found inside your dedicated Server inside `SCPSL_Data/Managed`.

Those are:
- Assembly-CSharp.dll
- Assembly-CSharp-firstpass.dll
- BouncyCastle.Cryptography.dll
- Mirror.dll
- Pooling.dll
- UnityEngine.AssetBundleModule.dll
- UnityEngine.CoreModule.dll
- UnityEngine.PhysicsModule.dll
- UnityEngine.UnityWebRequestModule.dll

Furthermore you also need the Exiled.Events.dll from [Exiled](https://github.com/Exiled-Official/EXILED/releases)

Lastly you need a tool like [AssemblyPublicizer](https://github.com/CabbageCrow/AssemblyPublicizer) to modify the following Assemblies:
- Assembly-CSharp.dll
- Mirror.dll
- Exiled.Events.dll

And then you are done!