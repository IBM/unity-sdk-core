# IBM Cloud Unity SDK Core
[![Build Status](https://travis-ci.org/IBM/unity-sdk-core.svg?branch=master)](https://travis-ci.org/IBM/unity-sdk-core/)
[![wdc-community.slack.com](https://wdc-slack-inviter.mybluemix.net/badge.svg)](http://wdc-slack-inviter.mybluemix.net/)
[![semantic-release](https://img.shields.io/badge/%20%20%F0%9F%93%A6%F0%9F%9A%80-semantic--release-e10079.svg)](https://github.com/semantic-release/semantic-release)
[![CLA assistant](https://cla-assistant.io/readme/badge/IBM/unity-sdk-core)](https://cla-assistant.io/IBM/unity-sdk-core)

The IBM Cloud Unity SDK Core is a core project of Unity SDKs generated using the IBM OpenAPI SDK generator. Generated services should use this package as a submodule of their generate Unity SDK.

<details>
  <summary>Table of Contents</summary>

  * [Before you begin](#before-you-begin)
  * [Configuring Unity](#configuring-unity)
  * [Getting the IBM Cloud Unity SDK Core and adding it to Unity](#getting-the-ibm-unity-sdk-core-and-adding-it-to-unity)
  * [Questions](#questions)
  * [Open Source @ IBM](#open-source--ibm)
  * [License](#license)
  * [Contributing](#contributing)

</details>

## Before you begin
Ensure that you have the following prerequisites:

* [Unity][get_unity]. You can use the **free** Personal edition.

## Configuring Unity
* Change the build settings in Unity (**File > Build Settings**) to any platform except for web player/Web GL. The IBM Watson SDK for Unity does not support Unity Web Player.
* If using Unity 2018.2 or later you'll need to set **Scripting Runtime Version** and **Api Compatibility Level** in Build Settings to **.NET 4.x equivalent**. We need to access security options to enable TLS 1.2. 

## Getting the IBM Unity SDK Core and adding it to Unity
The IBM Unity SDK Core is mean to be used as a submodule in other generated Unity SDKs. The selected SDK release will point to a particular commit of the IBM Unity SDK Core. Clone the SDK into your Unity project's Assets directory, and initalize and update submodules.

```bash
$ git clone [unity-sdk-url]
$ cd [unity-sdk-directory]
$ git submodule init
$ git submodule update
```

## Questions

If you are having difficulties using the APIs or have a question about the IBM Watson Services, please ask a question on
[dW Answers](https://developer.ibm.com/answers/questions/ask/?topics=watson)
or [Stack Overflow](http://stackoverflow.com/questions/ask?tags=ibm-watson).

## Open Source @ IBM
Find more open source projects on the [IBM Github Page](http://ibm.github.io/).

## License
This library is licensed under Apache 2.0. Full license text is available in [LICENSE](LICENSE).

## Contributing
See [CONTRIBUTING.md](.github/CONTRIBUTING.md).

[get_unity]: https://unity3d.com/get-unity
