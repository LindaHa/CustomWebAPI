CustomWebApi
=======================
Custom Kentico Module implemented as WebApi project

How to add the module to your Kentico solution
=======================
1. Clone the repository to your local machine
2. In VS reference the ```CustomWebApi.csproj``` from the repository in your Kentico Web App solution explorer (right click solution -> add -> existing project)
3. In VS reference the ```CustomWebApi``` project in ```CMSApp``` project (right click CMSApp project -> add -> reference -> Project tab -> check CustomWebApi)
4. Open Nuget Manager Console for the ```CustomWebApi``` project and execute ```enable-migrations``` followed by ```update-database```. The Entity Framework will by default try to connect to ```mssqllocaldb```. For more information see the ```App.config``` file.

Example usage
=======================
-to get JSON list of last 50 event logs Make a GET request to:

```
YOURDOMAIN/kenticoapi/system/show-eventlog
```

e.g.

```
http://localhost:8080/kenticoapi/system/show-eventlog
```