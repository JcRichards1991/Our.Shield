# README #

### Purpose ###
Provides active security for you Umbraco site using customizable components

---

### Why? ###
There is no end of ways that your Umbraco site can be compromised, scrapped or mistreated. A few of the more famous can be reduced with the addition of a security layer checking each and every request; whether its access to Umbraco admin pages, media files or anonymous api end points. Our.Shield integrates into the Umbraco backend to allow easy configuration of specialized modules, even across load balanced environments.

---

### Install ###
1. Install Our.Shield.Core Framework package via [NuGet](https://www.nuget.org/packages/Our.Shield.Core/):
```
PM> Install-Package Our.Shield.Core
```

2. Install the Our.Shield app(s) you desire via NuGet:
	* https://www.nuget.org/packages/Our.Shield.BackofficeAccess/
	```
	PM> Install-Package Our.Shield.BackofficeAccess
	```
		
	* https://www.nuget.org/packages/Our.Shield.MediaProtection/
	```
	PM> Install-Package Our.Shield.MediaProtection
	```

---

### Documentation ###

[Manual](https://github.com/JcRichards1991/Our.Shield/blob/master/Docs/manual.pdf)

---

### Log ###

**1.0.0 - 26/07/2017**

Our.Shield.Core, Our.Shield.BackofficeAccess & Our.Shield.MediaProtection released

---

### Future Development / Roadmap ###
* Multiple Environments - Configure different security for your dev, staging and live environment.
* Scrapper Defense - Stop bots from stealing your content and resources
* Geo Banning - Ban areas/countries/cloud services
* Google Safe Browsing - Disable dangerous Urls


### Source Code ###
Download the source code, it should work for Visual Studio 2013 & 2015. If you set **Our.Shield.TestSite** as your **Set as Startup project** this should execute the test Umbraco website, where you can test Our.Shield in different scenarios. Once running, surf to http://localhost:8560/umbraco and at the login type **admin** for user and **password** for password.
