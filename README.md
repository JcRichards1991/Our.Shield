# README #

### Purpose ###
Provides active security for you Umbraco site usign customizable components

---

### Why? ###
There is no end of ways that your Umbraco site can be compromised, scrapped or mistreated. A few of the more famous can be reduced with the addition of a security layer checking each and every request; whether its access to Umbraco admin pages, media files or anonymous api end points. Our.Shield integrates into the Umbraco backend to allow easy configuration, even across load balanced environments.

Because security is important!

---

### Install ###
1. Install Our.Shield.Core Framework package via (Add nuget link here to nuget shield package):
```
PM> Install-Package Our.Shield.Core
```

2. Install the Our.Shield app(s) you desire via NuGet:
	* (Add nuget link here to nuget backoffice access package)
	```
	PM> Install-Package Our.Shield.BackofficeAccess
	```
		
	* (Add nuget link here to nuget Media Protection package):
	```
	PM> Install-Package Our.Shield.MediaProtection
	```

---

### Documentation ###
Coming Soon!

	> Allows you to configure the backoffice access Url.
	> Allows you to restrict access to the configured backoffice access Url via a white-list of IP Address(es).
	
	> Allows you to setup member only media files.
	> Allows you to disable HotLinking - Stops other websites from serving your media.
	
---

### Log ###

**(Release Log item here)**

	(What happened in first release here)

---

### Future Development / Roadmap ###
* Multiple Environments - Configure different security for your dev, staging and live environment.
* Scrapper Defense - Stop bots from stealing your content and resources
* Geo Banning - Ban areas/countries/cloud services
* Google Safe Browsing - Disable dangerous Urls


### Source Code ###
Download the source code, it should work for Visual Studio 2013 & 2015. If you set **Our.Shield.TestSite** as your Startup project this should execute the test Umbraco website, where you can test Our.Shield in different scenarios. Once running, surf to http://localhost:8560/umbraco and at the login type **admin** for user and **password** for password.
