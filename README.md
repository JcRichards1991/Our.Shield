# README #

### Purpose ###
Provides active security for your Umbraco site using customizable components

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
	* https://www.nuget.org/packages/Our.Shield.FrontendAccess/
	```
	PM> Install-Package Our.Shield.FrontendAccess
	```
	* https://www.nuget.org/packages/Our.Shield.Elmah/
	```
	PM> Install-Package Our.Shield.Elmah
	```
---

### Documentation ###

[Manual](https://github.com/JcRichards1991/Our.Shield/blob/master/Docs/manual.pdf)

---

### Upgrading ###

**v1.0.0 to v1.0.1**

After upgrading, you will need to re-save the configurations of the installed apps

---

### Log ###
**1.0.6 - 25/03/2018**
* Shield Core: Fixes hanging state when creating a new environment
* Shield Core: Fixes issue with matching domain(s) to a request for custom environment(s)
* Shield core: Added custom configuration section to:
			   1) define request Headers for IP address checking to determine whether to grant or deny access
               2) Allows the ability to set the polling time for when shield checks it's app's configuration for changes.
                  This allows for a load balanced setup to keep slave server(s) in sync with the master server
				  
* Backoffice Access: Updated to take advantage of the custom configuration section added to the core

* Elmah: Fixes issues with allowing access for all IP Address
* Elmah: Localized the Reporting tab
* Elmah: Added Refresh Errors button to Reporting Tab
* Elmah: Added Generate Test Error button to Reporting Tab
* Elmah: Updated to take advantage of the custom configuration section added to the core

* Frontend Access: Fixes issues with allowing access for all IP Address
* Frontend Access: Updated to take advantage of the custom configuration section added to the core

* Media Protection: Updated to take advantage of the custom configuration section added to the core 

**1.0.5 - 08/01/2018**
* Release of new app Elmah. Adds the popular error logging library ELMAH to umbraco with the ability to add security restrictions to ~/elmah.axd

* Shield.Core: Added the ability for an app to have custom tabs for displaying additional information

**1.0.4 - 03/09/2017**
* Shield Core: Added the ability to subscribe to more of the http application cycle 
* Shield Core: UI improvements of shared assets/functionality between apps

* Backoffice Access: UI Improvements and added ability to add IP Address ranges as exception rules to the white or black list of access to the backoffice access url

* Frontend Access: UI Improvements and added ability to add IP Address ranges as exception rules to the white or black list of access to the front end

* Media Protection: Updates to work with Shield Core changes

**1.0.3 - 03/09/2017**

Update Shield framework to include the abilty to create, edit, delete & sort environments.

Release of Frontend Access Shield app

**1.0.2 - 17/08/2017**

Adds new setting to configuration to allow the ability to turn on/off the IP Addresses restrictions for Our.Shield.BackofficeAccess

**1.0.1 - 28/07/2017**

Fixes Our.Shield.Core migration not creating the Journal table, and therefore, not adding an entry to the umbracoMigration table

**1.0.0 - 26/07/2017**

Our.Shield.Core, Our.Shield.BackofficeAccess & Our.Shield.MediaProtection released

---

### Future Development / Roadmap ###
* Scrapper Defense - Stop bots from stealing your content and resources
* Geo Banning - Ban areas/countries/cloud services
* Google Safe Browsing - Disable dangerous Urls


### Source Code ###
Download the source code, it should work for Visual Studio 2013 & 2015. If you set **Our.Shield.Site** as your **Set as Startup project** this should execute the test Umbraco website, where you can test Our.Shield in different scenarios. Once running, surf to http://localhost:8560/admin and at the login type **admin** for user and **password1234!** for password.
