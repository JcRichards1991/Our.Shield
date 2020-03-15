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
	* https://www.nuget.org/packages/Our.Shield.Swagger/
	```
	PM> Install-Package Our.Shield.Swagger
	```
---

### Documentation ###

[Manual](https://github.com/JcRichards1991/Our.Shield/blob/master/Docs/manual.pdf)

---

### Upgrading ###

**v1.0.0 to v1.0.1**

After upgrading, you will need to re-save the configurations of the installed apps

---

### Change Log ###
**1.1.3 - 15/03/2020**
* Sheild Core: Fixed issue with AccessHelper class causing Membership authentication to not work correctly when Umbraco User is logged into Umbraco
* Shield Core: Fixed issue with Shield App Configurations not being applied on application startup.

* Backoffice Access: Added the ability to define an exclude list of URL(s) that Backoffice Access should ignore, even if it matches the backoffice Access Url. i.e. ~/umbraco/api

All Apps: Keeping version in sync with Shield Core

**1.1.2 - 08/03/2020**
* Shield Core: Fixed issue when used in a load balanced setup where not all servers in the cluster are kept in sync correctly

**1.1.1 - 28/02/2020**
* Frontend Access: Fixed issues with dependencies

* Swagger: Added the ability to define own Swagger Docs/UI Configs. See [docs](https://github.com/JcRichards1991/Our.Shield/blob/master/Docs/manual.pdf) for more information

Shield Core + All Apps: Updated to keep version in sync with Frontend access & Swagger

**1.1.0 - 01/09/2019**
* All: Lowered the target umbraco version. Now can be installed against 7.3.0+ (previously 7.5.4+).

* Shield Core: Refactoring of angular.
* Shield Core: Added ShieldService to allow external sources (apps) to retrieve limited information from the Shield Core.
* Shield Core: Added upper target Umbraco version to disallow installation on V8 sites and above (nuget installation).

* Backoffice Access: Fixed issue with the Backoffice always being accessible.
* Backoffice Access: Stopped the hard resetter file being created on every request when no configuration changes have been made.

* Elmah: Fixed issue where StackTrace not displaying when viewing more details of an issue when using with Elmah SQLServer nuget package.
* Elmah: Added domain filtering on the report tab. Now will only show errors that have occured for the domains defined on the environment. If no domains defined (i.e. default enviroment) errors are return as before.

* Frontend Access: Fixed issue disallowing access to the frontend when IP Address is whitelisted.

* Swagger: Release of new app, which adds [Swagger](https://swagger.io/) to your Umbraco installation, configured to filter out Umbraco's API's and allowing the ability to add security restrictions to ~/swagger

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
Download the source code, it should work for Visual Studio 2013 & 2015 & 2017. If you set **Our.Shield.Site** as your **Set as Startup project** this should execute the test Umbraco website, where you can test Our.Shield in different scenarios. Once running, surf to http://localhost:55034/umbraco and at the login type **admin** for user and **password1234!** for password.
