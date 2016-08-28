CustomWebApi
=======================
Custom Kentico Module implemented as WebApi project

How to add the module to your Kentico solution
=======================
1. Clone the repository to your local machine
2. In VS reference the ```CustomWebApi.csproj``` from the repository in your Kentico Web App solution explorer (right click solution -> add -> existing project)
3. In VS reference the ```CustomWebApi``` project in ```CMSApp``` project (right click CMSApp project -> add -> reference -> Project tab -> check CustomWebApi)

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