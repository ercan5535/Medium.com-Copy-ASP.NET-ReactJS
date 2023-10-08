# Description

It is a medium.com copy with ASP.NET micro services and React JS.<br>
Cookies are used for JWT Tokens storing.
<br>
<br>

<div>
  <img src="https://github.com/ercan5535/Medium.com-Copy-ASP.NET-ReactJS/assets/67562422/53b1f173-f476-4c4f-a13d-b255200830e8" width="500" height="400">
</div>


# Services

### User Service
<div>
  <img src="https://github.com/ercan5535/Medium.com-Copy-ASP.NET-ReactJS/assets/67562422/b12ca840-859f-4ed5-a5d7-93cec033b5ec" width="250" >
</div>

- Responsible for User Register/Login/Logut
- Responsible for Create/Refresh/Store/Blacklist JWT
- JWT operations handled by Helpers/JwtHelper.cs
- JWT parameters defined on appsettings.json (life times in minute)
```
  "JwtSecretKey": "MY-SECRET-KEY-123456789",
  "AccessTokenLifetime": 5,
  "RefreshTokenLifetime": 60
```

- Health Check 
```
{"status":"Healthy","totalDuration":"00:00:00.1951767","entries":{"Postgre SQL Server Check":{"data":{},"duration":"00:00:00.0882443","status":"Healthy","tags":["db","sql","postgres"]},"Redis Check":{"data":{},"duration":"00:00:00.1884614","status":"Healthy","tags":["redis"]}}}
```

- /UserService/check_user_db.sh ensure User Database is ready before migrations
```
command: sh -c "./check_user_db.sh && dotnet UserService.dll"
``` 

### Blog Service

<div>
  <img src="https://github.com/ercan5535/Medium.com-Copy-ASP.NET-ReactJS/assets/67562422/dde8f689-86c8-47f3-9c4d-1d743262d5e1" width="250" >
</div>

- Responsible for CRUD operations (blog, like, comment).
- Authenticate requests by checking tokens on cache
- Health Check (Custom healthcheck for mongo db at /BlogService/HealthCheck/MongoDBHealthCheck.cs)
```
{"status":"Healthy","totalDuration":"00:00:00.6547663","entries":{"MongoDB Check":{"data":{},"description":"MongoDB is reachable and has the expected collection.","duration":"00:00:00.6431295","status":"Healthy","tags":["Blog service mongo db"]},"Redis Check":{"data":{},"duration":"00:00:00.2648948","status":"Healthy","tags":["redis"]}}}
```

### Mongo Db
- Store Blog documents
- /MongoDb/mongo-init.js creates database, collection and blog examples

### Front-End
- It is a simple version of medium.com
- Blogs can be created cell by cell like on medium.com
- React-Bootstrap and MaterialUI is used for UI components
- React-markdown used for text displaying

Snapshot of Frontend(blog page with comment drawer)
<div>
  <img src="https://github.com/ercan5535/Medium.com-Copy-ASP.NET-ReactJS/assets/67562422/2afd1853-da9b-4056-8a9d-5d8dafe9f47a" width="800" height="400">
</div>


  
### API Gateway
- Serves static files
- Redirects reqeusts to relevant service

### Redis Cache
- Holds (Token, UserData) pairs
- Holds JWT Access tokens for validation from Blog Service
- Holds JWT Refresh tokens for blacklisting Refresh tokens


# Usage
```
docker-compose up 
```
command is enough to run all services <br>
NGINX will listen localhost:80 for serving react app <br>
You may login with Username: User1, Password: User1 or Register new account.
