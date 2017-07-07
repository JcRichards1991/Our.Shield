# README #

### Purpose ###
Provides security for you Umbraco site.

---

### Why? ###
Because security is important!

---

### Usage ###
1. Install Our.Shield.Core Framework package via (Add nuget link here to nuget shield package):
```
PM> Install-Package Our.Shield.Core
```

2. Install the Our.Shield app(s) you desire via NuGet:
	* (Add nuget link here to nuget backoffice access package)
	```
	PM> Install-Package Our.Shield.BackofficeAccess
	```
			* Allows you to configure the backoffice access Url.
			* Allows you to restrict access to the configured backoffice access Url via a white-list of IP Address(es).
		
	* (Add nuget link here to nuget Media Protection package):
	```
	PM> Install-Package Our.Shield.MediaProtection
	```
			* Allows you to setup member only media files.
			* Allows you to disable HotLinking - Stops other websites from serving your media.

3. Login to the Backoffice of your site; you should notice a new custom section called **_Shield_**. If you don't see this new section, make sure the user you're logged in as, has access to the **_Shield_** section via the user permissions within the User Section.

4. Configure the Our.Shield app(s) as you desire.

---

### Documentation ###
Coming Soon!

---

### Log ###

**(Release Log item here)**

	(What happened in first release here)

---

### Future Development / Roadmap ###
Coming Soon!
